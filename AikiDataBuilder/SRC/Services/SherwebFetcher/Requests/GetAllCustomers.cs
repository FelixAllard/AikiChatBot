using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using Newtonsoft.Json;
using Sherweb.Apis.ServiceProvider.Models;

namespace AikiDataBuilder.Services.SherwebFetcher.Requests;

/// <summary>
/// Get all Request :) First request done and might be modified
/// </summary>
public class GetAllCustomers : Request
{
    public GetAllCustomers(IHttpClientFactory clientFactory, SherwebDbContext sherwebDBContext) : base(clientFactory, sherwebDBContext)
    {
        Url = "https://api.sherweb.com/service-provider/v1/customers";
    }
    /// <summary>
    /// Adds the content to the database
    /// </summary>
    /// <param name="jsonContent">The content to serialize and then to add to the database</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task<OperationResult<JsonContent>> AddToDatabase(JsonContent jsonContent)
    {
        Exception exception = null;
        try
        {
            // Read the content as a string
            var jsonString = await jsonContent.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new InvalidOperationException("Received JSON is empty or null.");
            }

            // Deserialize the JSON string to C# objects using Json.NET
            var response = JsonConvert.DeserializeObject<Response>(jsonString);

            if (response == null)
            {
                throw new InvalidOperationException("Failed to deserialize the JSON content into the expected structure.");
            }

            foreach (var customer in response.Items)
            {
                //Add Every single customer to the database
                _sherwebDBContext.Customers.Add(new SherwebModel()
                {
                    Id = customer.Id,
                    DisplayName = customer.DisplayName,
                    Path = customer.Path
                });
            }
            
        }
        catch (JsonException jsonEx)
        {
            // Handle errors that occur during deserialization
            Console.Error.WriteLine($"Error deserializing JSON: {jsonEx.Message}");
            exception = new InvalidOperationException("There was an error processing the JSON content.", jsonEx);
        }
        catch (HttpRequestException httpEx)
        {
            // Handle any HTTP-related errors (if jsonContent is from an HTTP request)
            Console.Error.WriteLine($"HTTP error: {httpEx.Message}");
            exception = new InvalidOperationException("There was an error with the HTTP request.", httpEx);
        }
        catch (InvalidOperationException invalidOpEx)
        {
            // Handle any specific invalid operation issues (like empty content or failed deserialization)
            Console.Error.WriteLine($"Invalid operation: {invalidOpEx.Message}");
            exception = invalidOpEx;
        }
        catch (Exception ex)
        {
            // Catch any other unexpected exceptions
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            exception = new InvalidOperationException("An unexpected error occurred.", ex);
        }

        return await Task.FromResult(new OperationResult<JsonContent>()
        {
            Message = "Success in adding to database",
            Exception = exception,
            Result = jsonContent

        });
    }
}
/// <summary>
/// Serialisation Class
/// </summary>
public class Item
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public List<string> Path { get; set; }
}
/// <summary>
/// Serialisation Class
/// </summary>
public class Response
{
    public List<Item> Items { get; set; }
}