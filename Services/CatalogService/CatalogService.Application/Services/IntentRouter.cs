using System.Text.RegularExpressions;

namespace CatalogService.Application.Services;

// ─── Intent Enum ─────────────────────────────────────────────────────────────
public enum ChatIntent
{
    Track,       // "where is my order" / "track my order"
    Reorder,     // "last order" / "reorder"
    Menu,        // "show menu of Thai Orchid"
    Dish,        // "spicy food" / "show me pasta"
    Restaurant,  // "restaurants near me"
    Budget,      // "cheapest food under ₹200"
    TopRated,    // "best rated places"
    Fast,        // "fastest delivery"
    Cuisine,     // "I want Thai food"
    MealType,    // "breakfast options" / "desserts"
    Healthy,     // "something healthy / light"
    Combo,       // "what goes well with biryani?"
    Offers,      // "any deals today?"
    Support,     // "my order is wrong" / "refund"
    Chat,        // "hello" / "thanks" / "what can you do?"
    Ambiguous    // mixed transactional + search in same message
}

// ─── Filter Container ─────────────────────────────────────────────────────────
public class IntentFilters
{
    public string?  Query           { get; set; }
    public bool?    IsVeg           { get; set; }
    public decimal? MaxPrice        { get; set; }
    public decimal? MinPrice        { get; set; }
    public decimal? MinRating       { get; set; }
    public int?     MaxDeliveryTime { get; set; }
    public string?  CuisineType     { get; set; }
    public string?  RestaurantHint  { get; set; }
}

// ─── Router Result ────────────────────────────────────────────────────────────
public class IntentResult
{
    public ChatIntent    Intent  { get; set; }
    public IntentFilters Filters { get; set; } = new();
}

// ─── Static Intent Router ────────────────────────────────────────────────────
public static class IntentRouter
{
    private static readonly string[] ValidCuisines =
    {
        "Indian", "Thai", "Chinese", "Mexican", "Italian",
        "American", "Japanese", "Continental", "FastFood", "Vegan", "Mediterranean"
    };

    // Detects the primary intent from the user's last message.
    public static IntentResult Detect(string message)
    {
        var msg     = (message ?? "").ToLower().Trim();
        var filters = ParseFilters(msg);

        // ── Step 1: Identify all matching intents (ordered by priority) ──
        var matched = new List<ChatIntent>();

        if (IsTrack(msg))    matched.Add(ChatIntent.Track);
        if (IsReorder(msg))  matched.Add(ChatIntent.Reorder);
        if (IsMenu(msg))     matched.Add(ChatIntent.Menu);
        if (IsTopRated(msg)) matched.Add(ChatIntent.TopRated);
        if (IsFast(msg))     matched.Add(ChatIntent.Fast);
        if (IsHealthy(msg))  matched.Add(ChatIntent.Healthy);
        if (IsBudget(msg))   matched.Add(ChatIntent.Budget);
        if (IsCuisine(filters))  matched.Add(ChatIntent.Cuisine);
        if (IsMealType(msg)) matched.Add(ChatIntent.MealType);
        if (IsRestaurant(msg)) matched.Add(ChatIntent.Restaurant);
        if (IsDish(msg))     matched.Add(ChatIntent.Dish);
        if (IsSupport(msg))  matched.Add(ChatIntent.Support);
        if (IsOffers(msg))   matched.Add(ChatIntent.Offers);
        if (IsCombo(msg))    matched.Add(ChatIntent.Combo);

        // ── Step 2: Detect ambiguity ──
        // If the user asked for both a transactional action (track/reorder) AND a
        // food search in the same message → ask for clarification.
        var transactional = new[] { ChatIntent.Track, ChatIntent.Reorder };
        var searchable     = new[] { ChatIntent.Dish, ChatIntent.Restaurant, ChatIntent.Menu,
                                     ChatIntent.Budget, ChatIntent.TopRated, ChatIntent.Fast,
                                     ChatIntent.Cuisine, ChatIntent.MealType, ChatIntent.Healthy };

        bool hasTransactional = matched.Any(i => transactional.Contains(i));
        bool hasSearch        = matched.Any(i => searchable.Contains(i));

        if (hasTransactional && hasSearch)
            return new IntentResult { Intent = ChatIntent.Ambiguous, Filters = filters };

        // ── Step 3: Enrich filters based on specific intents ──
        var primary = matched.Count > 0 ? matched[0] : ChatIntent.Chat;

        if (primary == ChatIntent.TopRated)
            filters.MinRating = 4.0m;

        if (primary == ChatIntent.Fast)
            filters.MaxDeliveryTime ??= 25;

        if (primary == ChatIntent.Healthy)
        {
            filters.IsVeg ??= true;
            filters.Query ??= "salad bowl";
        }

        // For "menu of [restaurant name]", extract the restaurant name hint
        if (primary == ChatIntent.Menu)
        {
            var menuMatch = Regex.Match(msg, @"(?:menu of|menu at|items at|items from|show menu of|show .* menu)\s+(.+)");
            if (menuMatch.Success)
                filters.RestaurantHint = menuMatch.Groups[1].Value.Trim();
        }

        return new IntentResult { Intent = primary, Filters = filters };
    }

    // ─── Intent Matchers ─────────────────────────────────────────────────────

    private static bool IsTrack(string msg) =>
        Regex.IsMatch(msg, @"track|where is my order|order status|delivery status|where.*order|has.*picked up|when.*arrive|my order");

    private static bool IsReorder(string msg) =>
        Regex.IsMatch(msg, @"last order|previous order|reorder|same as before|order again|what did i order|my last|order what i had|had last");

    private static bool IsMenu(string msg) =>
        Regex.IsMatch(msg, @"menu of|menu at|items at|items from|what does .+ have|show .+ menu|what.*serve|their menu");

    private static bool IsTopRated(string msg) =>
        Regex.IsMatch(msg, @"best rated|top rated|highest rated|most popular|top restaurants|best restaurants|highly rated");

    private static bool IsFast(string msg) =>
        Regex.IsMatch(msg, @"fastest|quick delivery|fast delivery|in \d+ min|within \d+ min|speedy|asap|urgent");

    private static bool IsHealthy(string msg) =>
        Regex.IsMatch(msg, @"healthy|light meal|low cal|salad|bowl|nutritious|diet|fitness|clean eating");

    private static bool IsBudget(string msg) =>
        Regex.IsMatch(msg, @"cheap|affordable|budget|economical|inexpensive|pocket.friendly|low price|best value|under ₹|under rs|below \d+|less than \d+");

    private static bool IsCuisine(IntentFilters filters) =>
        filters.CuisineType != null;

    private static bool IsMealType(string msg) =>
        Regex.IsMatch(msg, @"\bbreakfast\b|\blunch\b|\bdinner\b|\bsupper\b|\bsnack\b|\bbrunch\b|\bdessert\b|\bsweet\b|\bsweets\b|\bdesert\b|\bdeserts\b");

    private static bool IsRestaurant(string msg) =>
        Regex.IsMatch(msg, @"\brestaurant\b|\bplace\b|\beat at\b|\bwhere to eat\b|\bfood place\b|\beating place\b");

    private static bool IsDish(string msg) =>
        Regex.IsMatch(msg, @"spicy|biryani|pizza|burger|paneer|chicken|food|dish|meal|pasta|noodle|sandwich|wrap|roll|curry|soup|rice|bread|roti|dosa|idli|thali|sushi|taco|fries|steak|kebab|tikka|korma|masala|momos|haleem|pav|bhaji|samosa|paratha|kulcha|fried|grilled|baked|roasted|steamed");

    private static bool IsSupport(string msg) =>
        Regex.IsMatch(msg, @"wrong order|refund|complaint|problem with|issue with|cancel|missing item|damaged|late delivery|help with order|not delivered|bad experience|dispute");

    private static bool IsOffers(string msg) =>
        Regex.IsMatch(msg, @"\bdeal\b|\bdeals\b|discount|offer|coupon|promo|voucher|cashback|savings|free delivery|sale");

    private static bool IsCombo(string msg) =>
        Regex.IsMatch(msg, @"goes well with|combo|pair with|suggest with|what to order with|complement|side dish|goes with|good combination");

    // ─── Filter Extractor ─────────────────────────────────────────────────────
    // Extracts structured parameters (veg, price, cuisine, etc.) from plain English.

    private static IntentFilters ParseFilters(string msg)
    {
        var filters = new IntentFilters();

        // ── Veg / Non-veg ────────────────────────────────────────────────────
        // Check non-veg keywords first; then check veg (only if non-veg not already matched)
        if (Regex.IsMatch(msg, @"\bnon.?veg\b|\bmeat\b|\begg\b|\bfish\b|\bprawn\b|\bmutton\b|\blamb\b|\bpork\b") ||
            (Regex.IsMatch(msg, @"\bchicken\b") && !Regex.IsMatch(msg, @"\bveg\b")))
            filters.IsVeg = false;
        else if (Regex.IsMatch(msg, @"\bveg\b|\bvegetarian\b|plant.based"))
            filters.IsVeg = true;

        // ── Max price ────────────────────────────────────────────────────────
        var maxPriceMatch = Regex.Match(msg, @"(?:under|below|less than|within|upto|up to|max)\s*[₹rs\.]*\s*(\d+)");
        if (maxPriceMatch.Success && decimal.TryParse(maxPriceMatch.Groups[1].Value, out var maxP))
            filters.MaxPrice = maxP;

        // ── Min price ────────────────────────────────────────────────────────
        var minPriceMatch = Regex.Match(msg, @"(?:above|more than|over|starting from|minimum|min)\s*[₹rs\.]*\s*(\d+)");
        if (minPriceMatch.Success && decimal.TryParse(minPriceMatch.Groups[1].Value, out var minP))
            filters.MinPrice = minP;

        // ── Max delivery time ─────────────────────────────────────────────────
        var deliveryMatch = Regex.Match(msg, @"(?:in|within|under)\s*(\d+)\s*min");
        if (deliveryMatch.Success && int.TryParse(deliveryMatch.Groups[1].Value, out var mins))
            filters.MaxDeliveryTime = mins;

        // ── Cuisine ───────────────────────────────────────────────────────────
        foreach (var cuisine in ValidCuisines)
        {
            if (msg.Contains(cuisine.ToLower()))
            {
                filters.CuisineType = cuisine;
                break;
            }
        }

        // ── Query keyword ─────────────────────────────────────────────────────
        // Strip known filter phrases to isolate the core keyword
        var query = msg;
        query = Regex.Replace(query, @"(?:under|below|less than|above|over|more than|within|in|upto|up to|minimum|max)\s*[₹rs\.]*\s*\d+(?:\.\d+)?\s*(?:rupees?|rs\.?|mins?|minutes?)?", "");
        query = Regex.Replace(query, @"\b(?:i want|i need|show me|give me|find me|get me|suggest|looking for|search for|can you|could you|would you|ca you|can u|could u|would u|please|pls|plz|some|somewhat|bit|a bit|kind of|sort of|so|like|want|to|eat|somting|something|anything|any|really|very|actually|dish|dishes|dishers)\b", "");
        query = Regex.Replace(query, @"\b(?:veg|vegetarian|non.?veg|plant.based)\b", "");
        foreach (var cuisine in ValidCuisines)
            query = Regex.Replace(query, @"\b" + cuisine.ToLower() + @"\b", "");
        query = Regex.Replace(query, @"\b(?:food|dish|meal|item|option|restaurant|restaurants|place|places)\b", "");
        
        // Strip intent keywords that would ruin text search
        query = Regex.Replace(query, @"\b(?:cheapest|cheap|affordable|budget|economical|best value|low price)\b", "");
        query = Regex.Replace(query, @"\b(?:best rated|top rated|highest rated|most popular|highly rated|best|rated)\b", "");
        query = Regex.Replace(query, @"\b(?:fastest|quick delivery|fast delivery|quick|fast|speedy|asap|urgent)\b", "");
        
        query = Regex.Replace(query, @"\s+", " ").Trim();

        if (!string.IsNullOrWhiteSpace(query) && query.Length > 2)
            filters.Query = query;

        // Bug fix: if a cuisine was detected but no meaningful dish keyword remains,
        // clear the query so the cuisine filter alone drives the search (prevents filler
        // words like "please" or "food" from polluting the search results).
        if (filters.CuisineType != null && !string.IsNullOrWhiteSpace(filters.Query))
        {
            var dishKeywords = new[] { "spicy", "veg", "cheap", "biryani", "pizza", "burger",
                "curry", "rice", "noodle", "soup", "salad", "paneer", "chicken", "pasta" };
            var hasDishKeyword = dishKeywords.Any(k => filters.Query!.Contains(k));
            if (!hasDishKeyword)
                filters.Query = null; // cuisine filter is enough
        }

        return filters;
    }
}
