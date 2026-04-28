using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogService.Application.DTOs.Ai;
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
    private readonly IConfiguration _config;
    private readonly ILogger<AiService> _logger;

    public AiService(HttpClient httpClient, ISearchService searchService, IConfiguration config, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _searchService = searchService;
        _config = config;
        _logger = logger;
    }

    public async Task<AiChatResponseDto> GetChatResponseAsync(AiChatRequestDto request)
    {
        var apiKey = _config["GEMINI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("GEMINI_API_KEY is not configured.");
            return new AiChatResponseDto { Text = "Sorry, my AI features are currently unavailable. Please check back later!" };
        }

        var responseDto = new AiChatResponseDto();

        var contents = new List<object>();
        foreach (var msg in request.Messages)
        {
            contents.Add(new
            {
                role = msg.Role == "model" ? "model" : "user",
                parts = new[] { new { text = msg.Text } }
            });
        }

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = "You are QuickBite's AI Assistant. Help users find restaurants and food. You can use the search_catalog tool to find restaurants. IMPORTANT: The 'query' parameter should ONLY contain specific keywords for names or food items (like 'Pizza', 'Burger', 'Subway'). DO NOT put adjectives like 'best', 'famous', 'tasty', or 'cheap' in the query parameter; instead, use filters like 'minRating' or leave the query empty to show all restaurants in a category. Keep your responses short, friendly, and helpful." } }
            },
            contents = contents,
            tools = new[]
            {
                new
                {
                    functionDeclarations = new[]
                    {
                        new
                        {
                            name = "search_catalog",
                            description = "Search for restaurants in the QuickBite catalog.",
                            parameters = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    query = new { type = "STRING", description = "Specific food item or restaurant name. Leave EMPTY for general category searches." },
                                    cuisineType = new { type = "STRING", description = "The style of food. One of: Italian, Chinese, Indian, Mexican, American, Thai, Japanese, Continental, FastFood, Vegan, Mediterranean" },
                                    minRating = new { type = "NUMBER", description = "Minimum rating (use 4 for 'best' or 'top rated')" }
                                }
                            }
                        }
                    }
                }
            }
        };

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            var responseString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gemini API error: {response.StatusCode} - {responseString}");
                return new AiChatResponseDto { Text = "Sorry, I'm having trouble thinking right now." };
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            var candidates = root.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return new AiChatResponseDto { Text = "I couldn't understand that." };

            var firstCandidate = candidates[0];
            var parts = firstCandidate.GetProperty("content").GetProperty("parts");
            
            bool isFunctionCall = false;
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("functionCall", out var functionCall))
                {
                    isFunctionCall = true;
                    var name = functionCall.GetProperty("name").GetString();
                    var args = functionCall.GetProperty("args");

                    if (name == "search_catalog")
                    {
                        var filter = new SearchRestaurantFilterDto();
                        
                        if (args.TryGetProperty("query", out var q)) filter.Query = q.GetString();
                        if (args.TryGetProperty("cuisineType", out var c) && Enum.TryParse<CuisineType>(c.GetString(), true, out var ct))
                        {
                            filter.CuisineType = ct;
                        }
                        if (args.TryGetProperty("minRating", out var r)) filter.MinRating = r.GetDecimal();

                        var results = await _searchService.AdvancedSearchAsync(filter);
                        responseDto.RecommendedRestaurants = results;

                        // Give a summary
                        if (results.Any())
                        {
                            var names = string.Join(", ", results.Take(3).Select(r => r.Name));
                            responseDto.Text = $"I found {results.Count} options for you! Here are some top picks: {names}.";
                        }
                        else
                        {
                            responseDto.Text = "I'm sorry, I couldn't find any restaurants matching your preferences right now.";
                        }
                    }
                    break;
                }
            }

            if (!isFunctionCall)
            {
                // Just text response
                var textPart = parts[0].GetProperty("text").GetString();
                responseDto.Text = textPart ?? "I'm here to help!";
            }

            return responseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return new AiChatResponseDto { Text = "Sorry, an unexpected error occurred." };
        }
    }
}
