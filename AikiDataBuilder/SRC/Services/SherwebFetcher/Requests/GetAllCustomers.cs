using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.Sherweb.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using AikiDataBuilder.Utilities;
using Sherweb.Apis.ServiceProvider.Models;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

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
    public override async Task<OperationResult<string>> AddToDatabase(string jsonContent)
    {
        Exception exception = null;
        try
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new InvalidOperationException("Received JSON is empty or null.");
            }

            Response? response;
            response = JsonSerializer.Deserialize<Response>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                throw new InvalidOperationException("Failed to deserialize the JSON content into the expected structure.");
            }
            
            // Drop all customers
            _sherwebDBContext.Customers.RemoveRange(_sherwebDBContext.Customers);
            foreach (var customer in response.Items)
            {
                // Add new customer
                _sherwebDBContext.Customers.Add(new SherwebModel()
                {
                    Id = customer.Id,
                    DisplayName = customer.DisplayName,
                    Path = customer.Path.ToList(),
                    SuspendedOn = customer.SuspendedOn
                });

            }

            await _sherwebDBContext.SaveChangesAsync(); // Ensure database changes are saved
        }
        catch (JsonException jsonEx)
        {
            Console.Error.WriteLine($"Error deserializing JSON: {jsonEx.Message}");
            exception = new InvalidOperationException("There was an error processing the JSON content.", jsonEx);
        }
        catch (HttpRequestException httpEx)
        {
            Console.Error.WriteLine($"HTTP error: {httpEx.Message}");
            exception = new InvalidOperationException("There was an error with the HTTP request.", httpEx);
        }
        catch (InvalidOperationException invalidOpEx)
        {
            Console.Error.WriteLine($"Invalid operation: {invalidOpEx.Message}");
            exception = invalidOpEx;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            exception = new InvalidOperationException("An unexpected error occurred.", ex);
        }

        if (exception != null)
            throw exception;

        return new OperationResult<string>()
        {
            Message = "Success in adding to database",
            Exception = exception,
            Result = jsonContent,
            Status = exception==null?OperationResultStatus.Success:OperationResultStatus.Critical
        };
    }
    /// <summary>
    /// Serialisation Class
    /// </summary>
    private class Item
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }

        [JsonConverter(typeof(StringOrArrayConverter))]
        public List<string> Path { get; set; } = new List<string>();

        public string SuspendedOn { get; set; }
    }

    /// <summary>
    /// Serialisation Class
    /// </summary>
    private class Response
    {
        public List<Item> Items { get; set; } = new List<Item>();
    }
}
