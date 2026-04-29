using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogService.Application.DTOs.Ai;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Services;

public class AiService : IAiService
{
    private readonly HttpClient          _httpClient;
    private readonly ISearchService      _searchService;
    private readonly IMenuItemService    _menuItemService;
    private readonly IRestaurantService  _restaurantService;
    private readonly IConfiguration     _config;
    private readonly ILogger<AiService> _logger;

    // ─── Base system identity (used in all LLM calls) ─────────────────────────
    private const string BaseSystemPrompt = @"
You are QuickBite's AI Ordering Assistant. You help users order food, find restaurants, and track orders.

STRICT RULES — follow these always:
1. You ONLY answer questions about food, restaurants, ordering, and order status on the QuickBite platform.
2. If the user asks about ANYTHING else (coding, general knowledge, personal advice, other topics) — politely decline and redirect: 'I can only help with food orders on QuickBite! What would you like to eat today?'
3. NEVER invent, hallucinate, or recall food items, restaurants, or prices from memory. Only use data from the [PRE-FETCHED DATA] block.
4. If no [PRE-FETCHED DATA] is provided, say you could not find results — never make up alternatives.
5. You CANNOT add items to the cart or place orders. If the user asks you to add something, tell them to click the 'Add' button next to the items. Do NOT pretend to add things to the cart.
6. Reply in plain text with markdown formatting (bold names, numbered lists). Keep replies short.
";

    // ─── Per-intent formatting instructions ───────────────────────────────────
    private static string GetFormattingInstruction(ChatIntent intent) => intent switch
    {
        ChatIntent.Dish or ChatIntent.Budget or ChatIntent.Cuisine
        or ChatIntent.MealType or ChatIntent.Healthy =>
            "List ONLY the menu items from the [PRE-FETCHED DATA] block above as a numbered list. " +
            "Format: **Item Name** — RestaurantName — ₹Price — 1-line description. " +
            "Do NOT add, invent, or recall any items from memory or previous conversation.",

        ChatIntent.Restaurant or ChatIntent.TopRated or ChatIntent.Fast =>
            "List ONLY the restaurants from the [PRE-FETCHED DATA] block above as a numbered list. " +
            "Format: **Restaurant Name** — Cuisine — Rating: X — Delivery: X mins. " +
            "Do NOT add, invent, or recall any restaurants from memory or previous conversation.",

        ChatIntent.Track =>
            "Using ONLY the data in [PRE-FETCHED DATA], summarise the order status warmly. " +
            "Include restaurant name, items ordered, current status, and total. If delayed, apologise briefly.",

        ChatIntent.Reorder =>
            "Using ONLY the data in [PRE-FETCHED DATA], confirm what the user last ordered and from which restaurant, " +
            "then ask if they'd like to reorder the same items.",

        ChatIntent.Menu =>
            "List ONLY the menu items from the [PRE-FETCHED DATA] block as a numbered list with **bold** " +
            "item names and prices. Do NOT add any items from memory.",

        ChatIntent.Ambiguous =>
            "The user seems to be asking about two different things at once. Politely ask them which " +
            "they would like help with first: finding food, or checking their order. Keep it very short.",

        ChatIntent.Support =>
            "You are a QuickBite customer support agent. Acknowledge the issue with empathy. " +
            "Tell the user to contact support at support@quickbite.com or use the Help section in the app. " +
            "Do NOT make promises about refunds or outcomes. Keep it brief.",

        ChatIntent.Offers =>
            "Tell the user about QuickBite's standard offers: free delivery on first order, " +
            "10% off on weekends, and loyalty points on every order. Keep it short and upbeat.",

        ChatIntent.Combo =>
            "Suggest 2-3 natural food pairings based on the user's message. Only suggest combinations " +
            "that make culinary sense. Keep it friendly and brief.",

        ChatIntent.Chat =>
            "The user said something unrelated to food ordering. Politely say you can only help with " +
            "food orders, restaurant discovery, and order tracking on QuickBite. " +
            "Then ask: 'What would you like to eat today?' Do NOT answer the off-topic question.",

        _ =>
            "Respond helpfully and conversationally as the QuickBite AI Assistant. Keep it short."
    };

    // ─── Constructor ──────────────────────────────────────────────────────────
    public AiService(
        HttpClient          httpClient,
        ISearchService      searchService,
        IMenuItemService    menuItemService,
        IRestaurantService  restaurantService,
        IConfiguration      config,
        ILogger<AiService>  logger)
    {
        _httpClient        = httpClient;
        _searchService     = searchService;
        _menuItemService   = menuItemService;
        _restaurantService = restaurantService;
        _config            = config;
        _logger            = logger;
    }

    private string LmBaseUrl => _config["LmStudio:BaseUrl"] ?? "http://127.0.0.1:1234";
    private string LmModel   => _config["LmStudio:Model"]   ?? "qwen/qwen3-1.7b";

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC ENTRY POINT
    // ═══════════════════════════════════════════════════════════════════════════
    public async Task<AiChatResponseDto> GetChatResponseAsync(
        AiChatRequestDto request,
        Guid?            userId    = null,
        string?          authToken = null)
    {
        // Build conversation history for the LLM
        var history = new List<object>
        {
            new { role = "system", content = BaseSystemPrompt }
        };

        foreach (var m in request.Messages)
        {
            var role = m.Role == "model" ? "assistant" : m.Role;
            history.Add(new { role, content = m.Text ?? string.Empty });
        }

        // Detect intent from the latest user message
        var lastUserText = request.Messages.LastOrDefault(m => m.Role == "user")?.Text ?? "";
        var intentResult = IntentRouter.Detect(lastUserText);

        _logger.LogInformation("[IntentRouter] Intent={Intent} Filters={@Filters}",
            intentResult.Intent, intentResult.Filters);

        return await HandleIntentFirstAsync(history, intentResult, userId, authToken);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CORE: Handle intent, pre-fetch data, then call LLM once to format
    // ═══════════════════════════════════════════════════════════════════════════
    private async Task<AiChatResponseDto> HandleIntentFirstAsync(
        List<object>  history,
        IntentResult  intentResult,
        Guid?         userId,
        string?       authToken)
    {
        var responseDto = new AiChatResponseDto();
        var intent      = intentResult.Intent;
        var filters     = intentResult.Filters;
        string?         toolData = null;

        try
        {
            switch (intent)
            {
                // ── Transactional: order tracking ─────────────────────────────
                case ChatIntent.Track:
                {
                    var result = await FetchOrderStatusDataAsync(userId, authToken, responseDto);
                    toolData = JsonSerializer.Serialize(result);
                    break;
                }

                // ── Transactional: last order / reorder ───────────────────────
                case ChatIntent.Reorder:
                {
                    var result = await FetchLatestOrderDataAsync(userId, authToken, responseDto);
                    toolData = JsonSerializer.Serialize(result);
                    break;
                }

                // ── Restaurant menu (2-step: catalog lookup → menu fetch) ──────
                case ChatIntent.Menu:
                {
                    var result = await FetchMenuByNameAsync(filters.RestaurantHint, responseDto);
                    toolData = JsonSerializer.Serialize(result);
                    break;
                }

                // ── All dish/food search intents ─────────────────────────────────
                case ChatIntent.Dish:
                case ChatIntent.Budget:
                case ChatIntent.Cuisine:
                case ChatIntent.MealType:
                case ChatIntent.Healthy:
                {
                    var result = await SearchMenuItemsByFiltersAsync(filters, intent, responseDto);
                    toolData = JsonSerializer.Serialize(result);
                    break;
                }

                // ── Restaurant search intents ─────────────────────────────────
                case ChatIntent.Restaurant:
                case ChatIntent.TopRated:
                case ChatIntent.Fast:
                {
                    var result = await SearchRestaurantsByFiltersAsync(filters, intent, responseDto);
                    toolData = JsonSerializer.Serialize(result);
                    break;
                }

                // ── Pure conversational (no tool) ─────────────────────────────
                case ChatIntent.Ambiguous:
                case ChatIntent.Support:
                case ChatIntent.Offers:
                case ChatIntent.Combo:
                case ChatIntent.Chat:
                default:
                    // No tool data — LLM handles purely conversationally
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HandleIntentFirst] Tool fetch failed for intent {Intent}", intent);
            // Gracefully fall through — LLM will handle without tool data
        }

        return await FormatWithLlmAsync(history, toolData, intent, responseDto);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LLM FORMATTING CALL (1 call, tool_choice: none, just formats the data)
    // ═══════════════════════════════════════════════════════════════════════════
    private async Task<AiChatResponseDto> FormatWithLlmAsync(
        List<object>      history,
        string?           toolData,
        ChatIntent        intent,
        AiChatResponseDto responseDto)
    {
        // Trim history to last 6 messages to prevent context poisoning from old turns
        // Always keep the system prompt (index 0) + last 6 user/assistant turns
        var trimmed = new List<object>();
        trimmed.Add(history[0]); // system prompt always first
        var conversationTurns = history.Skip(1).ToList();
        var keepFrom = Math.Max(0, conversationTurns.Count - 6);
        trimmed.AddRange(conversationTurns.Skip(keepFrom));
        var messages = trimmed;

        if (!string.IsNullOrEmpty(toolData))
        {
            // Inject the tool result as additional context — LLM sees it like given data
            messages.Add(new
            {
                role    = "system",
                content = $"[PRE-FETCHED DATA — use this to answer, do not call any tools]\n{toolData}"
            });
        }

        // Add the intent-specific formatting instruction last
        messages.Add(new
        {
            role    = "system",
            content = GetFormattingInstruction(intent)
        });

        var requestBody = new
        {
            model       = LmModel,
            messages,
            // No tools provided → model cannot call tools, only formats text
            max_tokens  = 1024,
            temperature = 0.5,
            stream      = false
        };

        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync(
                $"{LmBaseUrl}/v1/chat/completions", requestBody);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError("LM Studio error {Code}: {Body}", (int)httpResponse.StatusCode, err);
                responseDto.Text = "I'm having trouble responding right now. Please try again.";
                return responseDto;
            }

            var root    = (await JsonDocument.ParseAsync(
                await httpResponse.Content.ReadAsStreamAsync())).RootElement.Clone();

            var choice  = root.GetProperty("choices")[0];
            var message = choice.GetProperty("message");
            var content = message.TryGetProperty("content", out var c) ? c.GetString() : null;

            responseDto.Text = !string.IsNullOrWhiteSpace(content)
                ? content!.Trim()
                : "I found the data but couldn't format a response. Please try again.";

            // Detect token-truncation (finish_reason=length with empty/partial content)
            var finishReason = choice.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null;
            if (finishReason == "length" && string.IsNullOrWhiteSpace(responseDto.Text))
            {
                responseDto.Text = "Here are the results:\n" +
                    (toolData ?? "No data available.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LM Studio for formatting");
            responseDto.Text = "Connection error. Please try again.";
        }

        return responseDto;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DATA FETCHERS (called before LLM, results injected as context)
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<object> SearchMenuItemsByFiltersAsync(
        IntentFilters filters, ChatIntent intent, AiChatResponseDto responseDto)
    {
        CuisineType? cuisineType = null;
        if (filters.CuisineType != null &&
            Enum.TryParse<CuisineType>(filters.CuisineType, true, out var ct))
            cuisineType = ct;

        // Bug fix: when cuisineType is set but query is just filler (e.g. "food please"),
        // pass empty query so cuisine filter is the primary search driver.
        var effectiveQuery = filters.Query ?? "";
        if (cuisineType.HasValue && (effectiveQuery.Length < 3 ||
            effectiveQuery == "please" || effectiveQuery == "food please"))
            effectiveQuery = "";

        // Bug fix: for Budget intent (e.g. "cheapest food"), if no specific price was given
        // and query is empty, set a very high maxPrice so the DB fetches items, then sort them.
        if (intent == ChatIntent.Budget && !filters.MaxPrice.HasValue && !filters.MinPrice.HasValue && string.IsNullOrWhiteSpace(effectiveQuery))
        {
            filters.MaxPrice = 9999m; // dummy filter to bypass hasFilters=false check
        }

        _logger.LogInformation("[SearchMenuItems] query={Q} cuisine={C} maxPrice={MP} isVeg={V} intent={I}",
            effectiveQuery, filters.CuisineType, filters.MaxPrice, filters.IsVeg, intent);

        var results = await _searchService.SearchMenuItemsAsync(
            query:        effectiveQuery,
            maxPrice:     filters.MaxPrice,
            minPrice:     filters.MinPrice,
            restaurantId: null,
            cuisineType:  cuisineType,
            isVeg:        filters.IsVeg);

        // Sort by price ascending for budget intent
        if (intent == ChatIntent.Budget)
            results = results.OrderBy(i => i.Price).ToList();

        // Limit results to top 5 to avoid overwhelming the UI and LLM
        results = results.Take(5).ToList();
        
        responseDto.RecommendedMenuItems = results;

        if (!results.Any())
            return new { found = 0, message = "No items matched. Try broadening your search." };

        return new
        {
            found = results.Count,
            items = results.Select(i => new
            {
                i.Id, i.Name, i.Price, i.IsVeg,
                i.RestaurantId, i.RestaurantName, i.Description
            })
        };
    }

    private async Task<object> SearchRestaurantsByFiltersAsync(
        IntentFilters filters, ChatIntent intent, AiChatResponseDto responseDto)
    {
        var filter = new SearchRestaurantFilterDto
        {
            Query     = filters.Query,
            MinRating = filters.MinRating
        };

        if (filters.CuisineType != null &&
            Enum.TryParse<CuisineType>(filters.CuisineType, true, out var ct))
            filter.CuisineType = ct;

        // For top_rated/restaurant with no other filters: provide a non-null query
        // so GetFilteredAsync doesn't short-circuit on empty inputs
        if (string.IsNullOrWhiteSpace(filter.Query) && !filter.CuisineType.HasValue)
            filter.Query = ""; // empty string is fine — GetFilteredAsync handles it

        _logger.LogInformation("[SearchRestaurants] query={Q} minRating={R} cuisine={C} intent={I}",
            filter.Query, filter.MinRating, filter.CuisineType, intent);

        var results = await _searchService.AdvancedSearchAsync(filter);

        // If minRating produced 0 results, retry without the rating filter
        if (!results.Any() && filter.MinRating.HasValue)
        {
            _logger.LogInformation("[SearchRestaurants] 0 results with minRating, retrying without");
            filter.MinRating = null;
            results = await _searchService.AdvancedSearchAsync(filter);
        }

        // Sort by rating descending for top_rated intent
        if (intent == ChatIntent.TopRated)
            results = results.OrderByDescending(r => r.Rating).ToList();

        // Apply delivery time filter in-memory
        if (filters.MaxDeliveryTime.HasValue)
            results = results.Where(r => r.DeliveryTime <= filters.MaxDeliveryTime.Value).ToList();

        // Limit results to top 4 to avoid overwhelming the UI and LLM
        results = results.Take(4).ToList();

        responseDto.RecommendedRestaurants = results;

        if (!results.Any())
            return new { found = 0, message = "No restaurants found. Please try again." };

        return new
        {
            found       = results.Count,
            restaurants = results.Select(r => new
            {
                r.Id, r.Name,
                cuisine      = r.CuisineType.ToString(),
                r.Rating,
                r.DeliveryTime,
                r.City
            })
        };
    }

    private async Task<object> FetchMenuByNameAsync(
        string? restaurantHint, AiChatResponseDto responseDto)
    {
        if (string.IsNullOrWhiteSpace(restaurantHint))
            return new { error = "Please tell me which restaurant's menu you'd like to see." };

        // Step 1: find the restaurant by name
        var searchFilter = new SearchRestaurantFilterDto { Query = restaurantHint };
        var restaurants  = await _searchService.AdvancedSearchAsync(searchFilter);
        var restaurant   = restaurants.FirstOrDefault();

        if (restaurant == null)
            return new { error = $"I couldn't find a restaurant matching '{restaurantHint}'." };

        // Step 2: fetch that restaurant's menu
        var menu = await _menuItemService.GetMenuItemsByRestaurantAsync(restaurant.Id, "Customer");

        responseDto.RecommendedMenuItems = menu.Select(m => new MenuItemSearchResultDto
        {
            Id           = m.Id,
            Name         = m.Name,
            Description  = m.Description,
            Price        = m.Price,
            IsVeg        = m.IsVeg,
            RestaurantId = restaurant.Id,
            RestaurantName = restaurant.Name
        }).ToList();

        return new
        {
            restaurantName = restaurant.Name,
            found          = menu.Count,
            items          = menu.Select(m => new { m.Id, m.Name, m.Price, m.IsVeg, m.Description })
        };
    }

    private async Task<object> FetchOrderStatusDataAsync(
        Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        if (userId == null || string.IsNullOrEmpty(authToken))
            return new { error = "You need to be logged in to track an order." };

        var url = _config["OrderService:BaseUrl"] ?? "http://localhost:5003";
        return await FetchLatestOrderAsync(userId.Value, authToken, url, responseDto);
    }

    private async Task<object> FetchLatestOrderDataAsync(
        Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        if (userId == null || string.IsNullOrEmpty(authToken))
            return new { error = "You need to be logged in to view your order history." };

        var url = _config["OrderService:BaseUrl"] ?? "http://localhost:5003";
        return await FetchLatestOrderAsync(userId.Value, authToken, url, responseDto);
    }

    // ─── Order HTTP Helpers ───────────────────────────────────────────────────

    private async Task<object> FetchLatestOrderAsync(
        Guid userId, string authToken, string baseUrl, AiChatResponseDto responseDto)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync(
                $"{baseUrl}/gateway/orders?userId={userId}&activeOnly=false");

            if (!response.IsSuccessStatusCode)
                return new { error = "Could not retrieve your order history." };

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Array &&
                doc.RootElement.GetArrayLength() > 0)
                return await ParseOrderAsync(doc.RootElement[0].GetRawText(), responseDto);

            return new { found = 0, message = "No previous orders found." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch latest order for user {UserId}", userId);
            return new { error = "Order service is temporarily unavailable." };
        }
    }

    private async Task<object> ParseOrderAsync(string json, AiChatResponseDto responseDto)
    {
        using var doc       = JsonDocument.Parse(json);
        var orderData       = doc.RootElement.TryGetProperty("order", out var inner)
                                ? inner : doc.RootElement;

        string GetStr(string name) => FindProp(orderData, name)?.GetString() ?? "";
        decimal GetDec(string name) => FindProp(orderData, name)?.GetDecimal() ?? 0m;

        var orderId = GetStr("orderId");
        var status  = GetStr("orderStatus");
        var total   = GetDec("total");

        // Resolve restaurant name from Catalog
        string restaurantName = "the restaurant";
        if (FindProp(orderData, "restaurantId") is JsonElement rid &&
            Guid.TryParse(rid.GetString(), out var restaurantId))
        {
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
            if (restaurant != null) restaurantName = restaurant.Name;
        }

        // Collect item names
        var itemNames = new List<string>();
        if (FindProp(orderData, "items") is JsonElement items &&
            items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray())
                itemNames.Add(FindProp(item, "menuItemName")?.GetString() ?? "Item");
        }

        responseDto.OrderStatus = new OrderStatusInfoDto
        {
            OrderId        = Guid.TryParse(orderId, out var g) ? g : Guid.Empty,
            Status         = status,
            RestaurantName = restaurantName,
            ItemNames      = itemNames,
            Total          = total
        };

        return new { orderId, status, restaurantName, items = itemNames, total };
    }

    // ─── Utility ──────────────────────────────────────────────────────────────

    private static JsonElement? FindProp(JsonElement element, string name)
    {
        foreach (var prop in element.EnumerateObject())
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                return prop.Value;
        return null;
    }

    public async Task<bool> CheckConnectivityAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{LmBaseUrl}/api/v1/models");
            if (!response.IsSuccessStatusCode) return false;
            var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (!body.RootElement.TryGetProperty("models", out var models)) return false;
            foreach (var model in models.EnumerateArray())
                if (model.TryGetProperty("loaded_instances", out var inst) &&
                    inst.GetArrayLength() > 0)
                    return true;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("LM Studio check failed: {Msg}", ex.Message);
            return false;
        }
    }
}
