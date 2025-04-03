using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace AikiDataBuilder.Services.SherwebFetcher.Requests;

public class GetCustomerPlatformDetails : Request
{
    public GetCustomerPlatformDetails(IHttpClientFactory clientFactory) : base(clientFactory)
    {
    }

    public override async Task<OperationResult<string>> AddToDatabase(IServiceScope scope, string jsonContent)
    {
        var sherwebDbContext = scope.ServiceProvider.GetRequiredService<SherwebDbContext>();

        try
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new InvalidOperationException("Received JSON is empty or null.");
            }

            // Deserialize JSON into CustomerModel
            CustomerModel? customer = JsonSerializer.Deserialize<CustomerModel>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (customer == null)
            {
                throw new InvalidOperationException("Failed to deserialize the JSON content into the expected structure.");
            }

            var customerDetail = new CustomerDetail
           
            
            
            
            
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Failure($"Error processing JSON: {ex.Message}");
        }
    }

    public class PlatformDetails
    {
        public string PlatformId { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public class CustomerModel
    {
        public string CustomerId { get; set; } = string.Empty;
        public PlatformDetails PlatformDetails { get; set; } = new();
    }
}
