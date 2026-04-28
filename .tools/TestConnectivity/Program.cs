using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;

class Program
{
    static async Task Main()
    {
        string apiKey = "AIzaSyCxhrJWXYIX_bF4eaSZUbBgmZ1CDmUZ2F4";
        string model = "gemini-2.5-flash";

        Console.WriteLine($"Testing connectivity to {model}...");

        try
        {
            using var client = new Client(apiKey: apiKey);
            var response = await client.Models.GenerateContentAsync(
                model: model,
                contents: "Hi",
                config: null
            );

            if (response?.Candidates?.Count > 0)
            {
                Console.WriteLine("SUCCESS: Connectivity established.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED: {ex.GetType().Name} - {ex.Message}");
        }
    }
}
