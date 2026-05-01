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
    private readonly HttpClient _httpClient;
    private readonly ISearchService _searchService;
    private readonly IMenuItemService _menuItemService;
    private readonly IRestaurantService _restaurantService;
    private readonly IConfiguration _config;
    private readonly ILogger<AiService> _logger;

    private const string SystemPrompt = @"
You are QuickBite's AI Assistant, a powerful and friendly food ordering co-pilot.

CORE CAPABILITIES:
- Find restaurants(search by name) and browse menus.
- Search for specific dishes based on taste, price, or dietary needs.
- Track order status and check order history.
- Assist with general food-related inquiries.

GUIDELINES:
1. Be Conversational: Don't just dump lists. Explain why you're suggesting something.
2. Handle Nuance: Understand requests like 'less spicy', 'sweet', 'low budget'.
3. Search vs Cart: You can recommend items, but you CANNOT add them to the cart. Tell users to click the 'Add' button.
4. Data Integrity: Only talk about food/restaurants provided by your tools. If no results are found, say so politely.
5. Meta-Talk: If the user gives feedback on your personality, acknowledge it warmly and adapt.
6. Boundaries: Only discuss QuickBite food and orders. Decline off-topic requests politely.

Formatting: Use markdown (bold names, bullet points). Keep replies concise but high-quality.
";

    private static readonly object[] LmTools = new object[]
    {
        new {
            type = "function",
            function = new {
                name        = "search_restaurants",
                description = "Find restaurants based on name, cuisine, rating, or city.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "query",           new { type = "string", description = "Keyword for restaurant name." } },
                        { "cuisineType",     new { type = "string", description = "One of: Indian, Thai, Chinese, Mexican, Italian, American, Japanese, Continental, FastFood, Vegan, Mediterranean." } },
                        { "minRating",       new { type = "number", description = "Minimum rating (1.0 - 5.0)." } },
                        { "city",            new { type = "string", description = "City name." } },
                        { "maxDeliveryTime", new { type = "number", description = "Max delivery time in minutes." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "search_menu_items",
                description = "Find specific food items or dishes across all restaurants.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "query",        new { type = "string", description = "Food keyword (e.g. 'spicy', 'dessert', 'biryani')." } },
                        { "cuisineType",  new { type = "string", description = "Cuisine category." } },
                        { "maxPrice",     new { type = "number", description = "Maximum price in Rupees." } },
                        { "minPrice",     new { type = "number", description = "Minimum price in Rupees." } },
                        { "isVeg",        new { type = "boolean", description = "Filter for vegetarian items." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "get_restaurant_menu",
                description = "Get the full menu for a specific restaurant.",
                parameters  = new {
                    type       = "object",
                    required   = new[] { "restaurantId" },
                    properties = new Dictionary<string, object>
                    {
                        { "restaurantId", new { type = "string", description = "The UUID of the restaurant." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "track_order",
                description = "Get the current status of an order.",
                parameters  = new {
                    type       = "object",
                    properties = new Dictionary<string, object>
                    {
                        { "orderId", new { type = "string", description = "Optional Order UUID. If missing, gets latest order." } },
                    }
                }
            }
        },
        new {
            type = "function",
            function = new {
                name        = "get_order_history",
                description = "Retrieve the user's previous orders.",
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
        
        // Trim history to 10 turns
        var history = request.Messages.TakeLast(10).ToList();
        foreach (var m in history)
        {
            var role = m.Role == "model" ? "assistant" : m.Role;
            messages.Add(new { role, content = m.Text ?? string.Empty });
        }

        return await RunAgentLoopAsync(messages, userId, authToken);
    }

    private async Task<AiChatResponseDto> RunAgentLoopAsync(List<object> messages, Guid? userId, string? authToken, int maxSteps = 3)
    {
        var responseDto = new AiChatResponseDto();

        for (int step = 0; step < maxSteps; step++)
        {
            var requestBody = new
            {
                model       = LmModel,
                messages,
                tools       = LmTools,
                tool_choice = "auto",
                // Enable thinking for Qwen models
                thinking    = new { type = "enabled", budget_tokens = 2048 },
                max_tokens  = 4096,
                temperature = 0.2,
                stream      = false
            };

            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync($"{LmBaseUrl}/v1/chat/completions", requestBody);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("AI call failed: {Code}", httpResponse.StatusCode);
                    return new AiChatResponseDto { Text = "I'm experiencing a technical glitch. Please try again in a moment." };
                }

                var root = (await JsonDocument.ParseAsync(await httpResponse.Content.ReadAsStreamAsync())).RootElement;
                var choice = root.GetProperty("choices")[0];
                var message = choice.GetProperty("message");
                var finishReason = choice.GetProperty("finish_reason").GetString();
                var content = message.TryGetProperty("content", out var c) ? c.GetString() : null;

                if (finishReason != "tool_calls")
                {
                    responseDto.Text = !string.IsNullOrWhiteSpace(content) ? content.Trim() : "How else can I help you today?";
                    return responseDto;
                }

                // Handle Tool Calls
                var toolCalls = message.GetProperty("tool_calls");
                messages.Add(new { 
                    role = "assistant", 
                    content = content ?? string.Empty, 
                    tool_calls = JsonSerializer.Deserialize<List<JsonElement>>(toolCalls.GetRawText()) 
                });

                foreach (var tc in toolCalls.EnumerateArray())
                {
                    var toolCallId = tc.GetProperty("id").GetString() ?? "";
                    var fn = tc.GetProperty("function");
                    var toolName = fn.GetProperty("name").GetString() ?? "";
                    var argsRaw = fn.GetProperty("arguments").GetString() ?? "{}";
                    
                    _logger.LogInformation("[Agent] Step {Step}: Calling tool {Tool} with {Args}", step, toolName, argsRaw);
                    
                    var args = JsonDocument.Parse(argsRaw).RootElement;
                    var result = await ExecuteToolAsync(toolName, args, userId, authToken, responseDto);
                    
                    messages.Add(new { role = "tool", tool_call_id = toolCallId, content = JsonSerializer.Serialize(result) });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent loop failed");
                return new AiChatResponseDto { Text = "I'm having trouble processing that right now." };
            }
        }

        return responseDto;
    }

    private async Task<object> ExecuteToolAsync(string toolName, JsonElement args, Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        try
        {
            switch (toolName)
            {
                case "search_restaurants":  return await HandleSearchRestaurantsAsync(args, responseDto);
                case "search_menu_items":   return await HandleSearchMenuItemsAsync(args, responseDto);
                case "get_restaurant_menu": return await HandleGetMenuAsync(args, responseDto);
                case "track_order":         return await HandleTrackOrderAsync(args, userId, authToken, responseDto);
                case "get_order_history":   return await HandleTrackOrderAsync(args, userId, authToken, responseDto); // reused for history
                default:                    return new { error = $"Unknown tool: {toolName}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {Tool} execution failed", toolName);
            return new { error = "Data retrieval failed." };
        }
    }

    private async Task<object> HandleSearchRestaurantsAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        var filter = new SearchRestaurantFilterDto();
        if (args.TryGetProperty("query", out var q)) filter.Query = q.GetString();
        if (args.TryGetProperty("minRating", out var r)) filter.MinRating = r.GetDecimal();
        if (args.TryGetProperty("city", out var city)) filter.City = city.GetString();
        if (args.TryGetProperty("cuisineType", out var c) && Enum.TryParse<CuisineType>(c.GetString(), true, out var ct)) filter.CuisineType = ct;

        var results = await _searchService.AdvancedSearchAsync(filter);
        
        // Apply delivery time in-memory if needed
        if (args.TryGetProperty("maxDeliveryTime", out var dt))
            results = results.Where(r => r.DeliveryTime <= dt.GetInt32()).ToList();

        responseDto.RecommendedRestaurants = results.Take(4).ToList();
        return new { 
            found = results.Count, 
            restaurants = results.Take(4).Select(r => new { r.Id, r.Name, cuisine = r.CuisineType.ToString(), r.Rating, r.DeliveryTime, r.City }) 
        };
    }

    private async Task<object> HandleSearchMenuItemsAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        string query = args.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
        decimal? maxPrice = args.TryGetProperty("maxPrice", out var mp) ? mp.GetDecimal() : null;
        decimal? minPrice = args.TryGetProperty("minPrice", out var minp) ? minp.GetDecimal() : null;
        bool? isVeg = args.TryGetProperty("isVeg", out var veg) ? veg.GetBoolean() : null;
        CuisineType? cuisineType = (args.TryGetProperty("cuisineType", out var ct) && Enum.TryParse<CuisineType>(ct.GetString(), true, out var ctEnum)) ? ctEnum : null;

        var results = await _searchService.SearchMenuItemsAsync(query, maxPrice, minPrice, null, cuisineType, isVeg);
        responseDto.RecommendedMenuItems = results.Take(5).ToList();
        return new { 
            found = results.Count, 
            items = results.Take(5).Select(item => new { item.Id, item.Name, item.Price, item.IsVeg, item.RestaurantId, item.RestaurantName, item.Description }) 
        };
    }

    private async Task<object> HandleGetMenuAsync(JsonElement args, AiChatResponseDto responseDto)
    {
        if (!args.TryGetProperty("restaurantId", out var idProp) || !Guid.TryParse(idProp.GetString(), out var restaurantId)) 
            return new { error = "Valid restaurantId required" };

        var menu = await _menuItemService.GetMenuItemsByRestaurantAsync(restaurantId, "Customer");
        responseDto.RecommendedMenuItems = menu.Take(10).Select(m => new MenuItemSearchResultDto { 
            Id = m.Id, Name = m.Name, Description = m.Description, Price = m.Price, IsVeg = m.IsVeg, RestaurantId = restaurantId 
        }).ToList();
        
        return new { found = menu.Count, items = menu.Take(10).Select(m => new { m.Id, m.Name, m.Price, m.IsVeg, m.Description }) };
    }

    private async Task<object> HandleTrackOrderAsync(JsonElement args, Guid? userId, string? authToken, AiChatResponseDto responseDto)
    {
        if (userId == null || string.IsNullOrEmpty(authToken)) return new { error = "User must be logged in." };
        var url = _config["OrderService:BaseUrl"] ?? "http://localhost:5003";

        if (args.TryGetProperty("orderId", out var oid) && Guid.TryParse(oid.GetString(), out var orderId))
            return await FetchOrderDetailsAsync(orderId, authToken, url, responseDto);

        return await FetchLatestUserOrderAsync(userId.Value, authToken, url, responseDto);
    }

    private async Task<object> FetchOrderDetailsAsync(Guid orderId, string authToken, string baseUrl, AiChatResponseDto responseDto)
    {
        try {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var response = await client.GetAsync($"{baseUrl}/gateway/orders/{orderId}");
            if (!response.IsSuccessStatusCode) return new { error = "Order not found." };
            return await ParseOrderJsonAsync(await response.Content.ReadAsStringAsync(), responseDto);
        } catch { return new { error = "Order service unavailable." }; }
    }

    private async Task<object> FetchLatestUserOrderAsync(Guid userId, string authToken, string baseUrl, AiChatResponseDto responseDto)
    {
        try {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            var response = await client.GetAsync($"{baseUrl}/gateway/orders?userId={userId}&activeOnly=false");
            if (!response.IsSuccessStatusCode) return new { error = "Could not fetch orders." };
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0) 
                return await ParseOrderJsonAsync(doc.RootElement[0].GetRawText(), responseDto);
            return new { found = 0, message = "No orders found." };
        } catch { return new { error = "Order service unavailable." }; }
    }

    private async Task<object> ParseOrderJsonAsync(string json, AiChatResponseDto responseDto)
    {
        using var doc = JsonDocument.Parse(json);
        var orderData = doc.RootElement.TryGetProperty("order", out var inner) ? inner : doc.RootElement;
        
        string GetStr(string name) => FindProp(orderData, name)?.GetString() ?? "";
        decimal GetDec(string name) => FindProp(orderData, name)?.GetDecimal() ?? 0m;

        var orderId = GetStr("orderId");
        var status = GetStr("orderStatus");
        var total = GetDec("total");
        
        string restaurantName = "the restaurant";
        if (FindProp(orderData, "restaurantId") is JsonElement rid && Guid.TryParse(rid.GetString(), out var restaurantId))
        {
            try 
            {
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(restaurantId);
                if (restaurant != null) restaurantName = restaurant.Name;
            }
            catch (Exception ex)
            {
                // If restaurant is deactivated, GetRestaurantByIdAsync throws RestaurantNotFoundException.
                // We should just use the default "the restaurant" string instead of crashing the order tracking.
                _logger.LogWarning(ex, "Could not fetch restaurant details for AI order tracking for restaurant ID {RestaurantId}. Proceeding with default name.", restaurantId);
            }
        }

        var itemNames = new List<string>();
        if (FindProp(orderData, "items") is JsonElement items && items.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in items.EnumerateArray()) 
                itemNames.Add(FindProp(item, "menuItemName")?.GetString() ?? "Item");
        }

        responseDto.OrderStatus = new OrderStatusInfoDto { 
            OrderId = Guid.TryParse(orderId, out var g) ? g : Guid.Empty, 
            Status = status, RestaurantName = restaurantName, ItemNames = itemNames, Total = total 
        };
        return new { orderId, status, restaurantName, items = itemNames, total };
    }

    private static JsonElement? FindProp(JsonElement element, string name)
    {
        foreach (var prop in element.EnumerateObject()) 
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)) return prop.Value;
        return null;
    }

    public async Task<bool> CheckConnectivityAsync() 
    {
        try {
            var response = await _httpClient.GetAsync($"{LmBaseUrl}/api/v1/models");
            return response.IsSuccessStatusCode;
        } catch { return false; }
    }
}
