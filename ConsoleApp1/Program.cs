using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7089/notificationHub")
            .Build();

        connection.On<JsonElement>("NewSearch", (json) =>
        {
            try
            {
                Console.WriteLine($"New search received:");
                Console.WriteLine($"User: {json.GetProperty("user").GetString()}");
                Console.WriteLine($"Location: {json.GetProperty("latitude").GetDouble()}, {json.GetProperty("longitude").GetDouble()}");
                Console.WriteLine($"Radius: {json.GetProperty("radius").GetInt32()}m");
                Console.WriteLine($"Category: {json.GetProperty("category").GetString() ?? "None"}");
                Console.WriteLine($"Results: {json.GetProperty("resultsCount").GetInt32()}");
                Console.WriteLine($"Timestamp: {json.GetProperty("timestamp").GetString()}");
                Console.WriteLine("----------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        });

        try
        {
            await connection.StartAsync();
            await connection.InvokeAsync("SubscribeToSearches");
            Console.WriteLine("Connected to SignalR Hub. Waiting for search notifications...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
        }

        Console.ReadLine();
    }
}