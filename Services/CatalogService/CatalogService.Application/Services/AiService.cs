using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly HttpClient _httpClient;
    private readonly ISearchService _searchService;
    private readonly IMenuItemService _menuItemService;
    private readonly IRestaurantService _restaurantService;
    private readonly IConfiguration _config;
    private readonly ILogger<AiService> _logger;

    private const string SystemPrompt = @"
You are the AI Ordering Assistant for QuickBite, a Food Delivery platform.

 ==================================================
 CORE ROLE
 ==================================================
You are an intelligent food ordering copilot that helps users find food, track orders, and manage their cart.

 ==================================================
 TONE & PERSONALITY
 ==================================================
Be Friendly, Fast, and Helpful. Keep replies short.

 ==================================================
 SEARCH LOGIC (CRITICAL)
 ==================================================
- Do NOT assume Indian cuisine. Unless the user explicitly names a cuisine (e.g. Mexican, Chinese), always leave cuisineType null in your tool calls.
- Do NOT assume Veg unless the user asks for it.
- If a user asks for something spicy, use search_menu_items with query=""spicy"".
- If a user asks for best or top rated, set minRating=4.0 in search_catalog.
- Always prefer showing 3-5 high-quality options.

 ==================================================
 FORMATTING
 ==================================================
When listing items, ALWAYS use this format:
1. **Dish Name / Restaurant** — Price — Description

 ==================================================
 TOOL USAGE
 ==================================================
- Use 'search_menu_items' for food/dishes.
- Use 'search_catalog' for restaurants.
- Use 'get_order_status' for tracking.
- Use 'get_last_order' for reordering.

Valid Cuisines: Italian, Chinese, Indian, Mexican, American, Thai, Japanese, Continental, FastFood, Vegan, Mediterranean.
";

    private static readonly object[] LmTools = new object[]
    {
        new {
            type = "function",
            function = new {
                name        = "search_catalog",
                description = "Search for restaurants.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "query",           new { type = "string", description = "Optional keyword for restaurant name." } },
                        { "cuisineType",     new { type = "string", description = "Optional. One of the valid cuisines." } },
                        { "minRating",       new { type = "number", description = "Optional. Min 1-5 rating." } },
                        { "city",            new { type = "string", description = "Optional. City name." } },
                        { "maxDeliveryTime", new { type = "number", description = "Optional. Max delivery time in mins." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "search_menu_items",
                description = "Search for specific food items.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "query",        new { type = "string", description = "Optional keyword (e.g. 'spicy', 'paneer')." } },
                        { "cuisineType",  new { type = "string", description = "Optional cuisine filter." } },
                        { "maxPrice",     new { type = "number", description = "Optional max budget." } },
                        { "minPrice",     new { type = "number", description = "Optional min budget." } },
                        { "isVeg",        new { type = "boolean", description = "Optional veg/non-veg filter." } },
                        { "restaurantId", new { type = "string", description = "Optional restaurant UUID." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "get_restaurant_menu",
                description = "Get all menu items for a restaurant.",
                parameters  = new {
                    type       = "object",
                    required   = new[] { "restaurantId" },
                    properties = new Dictionary<string, object>
                    {
                        { "restaurantId", new { type = "string" } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "get_order_status",
                description = "Get status of an order.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "orderId", new { type = "string" } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "get_last_order",
                description = "Retrieve last order.",
                parameters  = new { type = "object", properties = new { } }
            }
        }
    };

    public AiService(
        HttpClient httpClient,
        ISearchService searchService,
        IMenuItemService menuItemService,
        IRestaurantService restaurantService,
        IConfiguration config,
        ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _searchService = searchService;
        _menuItemService = menuItemService;
        _restaurantService = restaurantService;
        _config = config;
        _logger = logger;
    }

    private string LmBaseUrl => _config["LmStudio:BaseUrl"] ?? "http://127.0.0.1:1234";
    private string LmModel   => _config["LmStudio:Model"]   ?? "qwen/qwen3-1.7b";

    public async Task<AiChatResponseDto> GetChatResponseAsync(AiChatRequestDto request, Guid? userId = null, string? authToken = null)
    {
        var messages = new List<object> { new { role = "system", content = SystemPrompt } };
        foreach (var m in request.Messages)
        {
            var role = m.Role == "model" ? "assistant" : m.Role;
            messages.Add(new { role, content = m.Text ?? string.Empty });
        }
        return await RunLocalLlmLoopAsync(messages, userId, authToken);
    }

    private async Task<AiChatResponseDto> RunLocalLlmLoopAsync(List<object> messages, Guid? userId, string? authToken, int maxIterations = 5)
    {
        var responseDto = new AiChatResponseDto();
        for (int i = 0; i < maxIterations; i++)
        {
            var requestBody = new
            {
                model       = LmModel,
                messages,
                tools       = LmTools,
                tool_choice = "auto",
                thinking    = new { type = "enabled", budget_tokens = 2048 },
                max_tokens  = 4096,
                temperature = 0.7,
                stream      = false
            };
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync($"{LmBaseUrl}/v1/chat/completions", requestBody);
                if (!httpResponse.IsSuccessStatusCode) return new AiChatResponseDto { Text = "I'm having trouble thinking right now." };
                var root = (await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync())).RootElement.Clone();
                var choice = root.GetProperty("choices")[0];
                var finishReason = choice.GetProperty("finish_reason").GetString();
                var message = choice.GetProperty("message");
                var content = message.TryGetProperty("content", out var c) ? c.GetString() : null;

                if (finishReason != "tool_calls")
                {
                    responseDto.Text = !string.IsNullOrWhiteSpace(content) ? content! : "How else can I help you today?";
                    return responseDto;
                }

                var toolCalls = message.GetProperty("tool_calls");
                messages.Add(new { role = "assistant", content = content ?? string.Empty, tool_calls = JsonSerializer.Deserialize<List<JsonElement>>(toolCalls.GetRawText()) });

                foreach (var tc in toolCalls.EnumerateArray())
                {
                    var toolCallId = tc.GetProperty("id").GetString() ?? "";
                    var fn = tc.GetProperty("function");
                    var toolName = fn.GetProperty("name").GetString() ?? "";
                    var argsRaw = fn.GetProperty("arguments").GetString() ?? "{}";
                    var args = JsonDocument.Parse(argsRaw).RootElement.Clone();
                    var result = await ExecuteToolAsync(toolName, args, userId, authToken, responseDto);
                    messages.Add(new { role = "tool", tool_call_id = toolCallId, content = JsonSerializer.Serialize(result) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling LM Studio");
                return new AiChatResponseDto { Text = "Connection error." };
            }
        }
        return new AiChatResponseDto { Text = "No response formulated." };
    }

    private async Task<object> ExecuteToolAsync(string toolName, JsonElement args, Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        try
        {
            switch (toolName)
            {
                case "search_catalog":      return await SearchCatalogAsync(args, responseDto);
                case "search_menu_items":   return await SearchMenuItemsAsync(args, responseDto);
                case "get_restaurant_menu": return await GetRestaurantMenuAsync(args, responseDto);
                case "get_order_status":    return await GetOrderStatusAsync(args, userId, authToken, responseDto);
                case "get_last_order":      return await GetLastOrderAsync(userId, authToken, responseDto);
                default:                    return new { error = $"Unknown tool: {toolName}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {Tool}", toolName);
            return new { error = "Tool failed." };
        }
    }

    private async Task<object> SearchCatalogAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        var filter = new SearchRestaurantFilterDto();
        if (args.TryGetProperty("query", out var q)) filter.Query = q.GetString();
        if (args.TryGetProperty("minRating", out var r)) filter.MinRating = r.GetDecimal();
        if (args.TryGetProperty("city", out var city)) filter.City = city.GetString();
        if (args.TryGetProperty("cuisineType", out var c) && Enum.TryParse<CuisineType>(c.GetString(), true, out var ct)) filter.CuisineType = ct;

        var results = await _searchService.AdvancedSearchAsync(filter);
        responseDto.RecommendedRestaurants = results;
        return new { found = results.Count, restaurants = results.Select(r => new { r.Id, r.Name, cuisine = r.CuisineType.ToString(), r.Rating, r.DeliveryTime, r.City }) };
    }

    private async Task<object> SearchMenuItemsAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        string query = args.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
        decimal? maxPrice = args.TryGetProperty("maxPrice", out var mp) ? mp.GetDecimal() : null;
        decimal? minPrice = args.TryGetProperty("minPrice", out var minp) ? minp.GetDecimal() : null;
        bool? isVeg = args.TryGetProperty("isVeg", out var veg) ? veg.GetBoolean() : null;
        Guid? restaurantId = (args.TryGetProperty("restaurantId", out var rid) && Guid.TryParse(rid.GetString(), out var gid)) ? gid : null;
        CuisineType? cuisineType = (args.TryGetProperty("cuisineType", out var ct) && Enum.TryParse<CuisineType>(ct.GetString(), true, out var ctEnum)) ? ctEnum : null;

        var results = await _searchService.SearchMenuItemsAsync(query, maxPrice, minPrice, restaurantId, cuisineType, isVeg);
        responseDto.RecommendedMenuItems = results;
        return new { found = results.Count, items = results.Select(item => new { item.Id, item.Name, item.Price, item.IsVeg, item.RestaurantId, item.RestaurantName, item.Description }) };
    }

    private async Task<object> GetRestaurantMenuAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        if (!args.TryGetProperty("restaurantId", out var idProp) || !Guid.TryParse(idProp.GetString(), out var restaurantId)) return new { error = "restaurantId required" };
        var menu = await _menuItemService.GetMenuItemsByRestaurantAsync(restaurantId, "Customer");
        responseDto.RecommendedMenuItems = menu.Select(m => new MenuItemSearchResultDto { Id = m.Id, Name = m.Name, Description = m.Description, Price = m.Price, IsVeg = m.IsVeg, RestaurantId = restaurantId }).ToList();
        return new { found = menu.Count, items = menu.Select(m => new { m.Id, m.Name, m.Price, m.IsVeg, m.Description }) };
    }

    private async Task<object> GetOrderStatusAsync(JsonElement args, Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        if (userId == null || string.IsNullOrEmpty(authToken)) return new { error = "Login required" };
        var url = _config["OrderService:BaseUrl"] ?? "http://localhost:5003";
        if (args.TryGetProperty("orderId", out var oid) && Guid.TryParse(oid.GetString(), out var orderId)) return await FetchOrderByIdAsync(orderId, authToken, url, responseDto);
        return await FetchLatestOrderAsync(userId.Value, authToken, url, responseDto);
    }

    private async Task<object> GetLastOrderAsync(Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        if (userId == null || string.IsNullOrEmpty(authToken)) return new { error = "Login required" };
        var url = _config["OrderService:BaseUrl"] ?? "http://localhost:5003";
        return await FetchLatestOrderAsync(userId.Value, authToken, url, responseDto);
    }

    private async Task<object> FetchOrderByIdAsync(Guid orderId, string authToken, string baseUrl, AiChatResponseDto responseDto)
    {
        try {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var response = await client.GetAsync($"{baseUrl}/gateway/orders/{orderId}");
            if (!response.IsSuccessStatusCode) return new { error = "Not found" };
            return await ParseOrderResponseAsync(await response.Content.ReadAsStringAsync(), responseDto);
        } catch { return new { error = "Order service unavailable" }; }
    }

    private async Task<object> FetchLatestOrderAsync(Guid userId, string authToken, string baseUrl, AiChatResponseDto responseDto)
    {
        try {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var response = await client.GetAsync($"{baseUrl}/gateway/orders?userId={userId}&activeOnly=false");
            if (!response.IsSuccessStatusCode) return new { error = "Error fetching history" };
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0) return await ParseOrderResponseAsync(doc.RootElement[0].GetRawText(), responseDto);
            return new { found = 0 };
        } catch { return new { error = "Order service unavailable" }; }
    }

    private async Task<object> ParseOrderResponseAsync(string json, AiChatResponseDto responseDto)
    {
        using var doc = JsonDocument.Parse(json);
        var orderData = doc.RootElement.TryGetProperty("order", out var inner) ? inner : doc.RootElement;
        
        string GetStr(string name) => FindProperty(orderData, name)?.GetString() ?? "";
        decimal GetDec(string name) => FindProperty(orderData, name)?.GetDecimal() ?? 0m;

        var orderId = GetStr("orderId");
        var status = GetStr("orderStatus");
        var total = GetDec("total");
        
        string restaurantName = "the restaurant";
        if (FindProperty(orderData, "restaurantId") is JsonElement rid && Guid.TryParse(rid.GetString(), out var restaurantId))
        {
            var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
            if (restaurant != null) restaurantName = restaurant.Name;
        }

        var itemNames = new List<string>();
        if (FindProperty(orderData, "items") is JsonElement items && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray()) itemNames.Add(FindProperty(item, "menuItemName")?.GetString() ?? "Menu Item");
        }

        responseDto.OrderStatus = new OrderStatusInfoDto { OrderId = Guid.TryParse(orderId, out var g) ? g : Guid.Empty, Status = status, RestaurantName = restaurantName, ItemNames = itemNames, Total = total };
        return new { orderId, status, restaurantName, items = itemNames, total };
    }

    private static JsonElement? FindProperty(JsonElement element, string name)
    {
        foreach (var prop in element.EnumerateObject()) if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)) return prop.Value;
        return null;
    }

    public async Task<bool> CheckConnectivityAsync() { return true; }
}
