using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.Sherweb.Database;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using AikiDataBuilder.Utilities;
using AikiDataBuilder.Utilities.EnumConverter;
using Sherweb.Apis.ServiceProvider.Models;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using Subscription = AikiDataBuilder.Model.Sherweb.Database.Subscription;
using SubscriptionFees = AikiDataBuilder.Model.Sherweb.Database.SubscriptionFees;
using CommitmentTermDb = AikiDataBuilder.Model.Sherweb.Database.CommitmentTerm;
using RenewalConfigurationDb = AikiDataBuilder.Model.Sherweb.Database.RenewalConfiguration;

namespace AikiDataBuilder.Services.SherwebFetcher.Requests;

/// <summary>
/// Get all Request :) First request done and might be modified
/// </summary>
public class GetSubscriptions : Request
{
    public GetSubscriptions(HttpClient clientFactory, SherwebDbContext sherwebDBContext) : base(clientFactory, sherwebDBContext)
    {
        Url = "https://api.sherweb.com/service-provider/v1/billing/subscriptions?customerId={{customerId}}";
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

            ApiResponse? subscriptions = JsonSerializer.Deserialize<ApiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new CommitmentLenghtConverter() } // Register the custom converter
            });
            
            if (subscriptions == null)
            {
                throw new InvalidOperationException("Failed to deserialize the JSON content into the expected structure.");
            }
            
            // Drop all existing charges
            _sherwebDBContext.Subscriptions.RemoveRange(_sherwebDBContext.Subscriptions);
            _sherwebDBContext.SubscriptionFees.RemoveRange(_sherwebDBContext.SubscriptionFees);
            _sherwebDBContext.CommitmentTerms.RemoveRange(_sherwebDBContext.CommitmentTerms);
            _sherwebDBContext.RenewalConfigurations.RemoveRange(_sherwebDBContext.RenewalConfigurations);
            _sherwebDBContext.CommittedMinimalQuantities.RemoveRange(_sherwebDBContext.CommittedMinimalQuantities);
            
            
            foreach (var subscription in subscriptions.Items)
            {
                var subscriptionDbInstance = _sherwebDBContext.Subscriptions.Add(new Subscription()
                {
                    Id = subscription.Id,
                    ProductName = subscription.ProductName,
                    Description = subscription.Description,
                    Sku = subscription.Sku,
                    Quantity = subscription.Quantity,
                    BillingCycle = subscription.BillingCycle,
                    PurchaseDate = subscription.PurchaseDate,
                    Fees = new SubscriptionFees()
                    {
                        Currency = subscription.Fees.Currency,
                        SetupFee = subscription.Fees.SetupFee,
                        RecurringFee = subscription.Fees.RecurringFee,
                    },
                    CommitmentTerm = new CommitmentTermDb()
                    {
                       Type = subscription.CommitmentTerm.Type,
                       TermEndDate = subscription.CommitmentTerm.TermEndDate,
                       RenewalConfiguration = new RenewalConfigurationDb()
                       {
                           RenewalDate = subscription.CommitmentTerm.RenewalConfiguration.RenewalDate,
                           ScheduledQuantity = subscription.CommitmentTerm.RenewalConfiguration.ScheduledQuantity
                       }
                    }
                });
                foreach (var committedMinimalQuantity in subscription.CommitmentTerm.CommittedMinimalQuantities)
                {
                    subscriptionDbInstance.Entity.CommitmentTerm.CommittedMinimalQuantities.Add(
                        new AikiDataBuilder.Model.Sherweb.Database.CommittedMinimalQuantity()
                        {
                            Quantity = committedMinimalQuantity.Quantity,
                            CommittedUntil = committedMinimalQuantity.CommittedUntil,
                        }
                    );
                }
            }

            await _sherwebDBContext.SaveChangesAsync(); // Ensure database changes are saved
        }
        catch (JsonException jsonEx)
        {
            Console.Error.WriteLine($"Error deserializing JSON: {jsonEx.Message}{jsonEx.LineNumber} {jsonEx.StackTrace}");
            exception = new InvalidOperationException("There was an error processing the JSON content.", jsonEx);
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

        return new OperationResult<string>()
        {
            Message = exception==null?"Successfully Added to database":$"Failed to add to database : {exception.GetBaseException()}\n{exception.StackTrace}",
            Exception = exception,
            Result = jsonContent,
            Status = exception==null?OperationResultStatus.Success:OperationResultStatus.Critical
        };
    }
    //Here are the serialization classes

    private class ApiResponse
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; } = new();
    }

    private class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
        [JsonPropertyName("billingCycle")]
        public string BillingCycle { get; set; }
        
        [JsonPropertyName("purchaseDate")]
        public string PurchaseDate { get; set; }
        
        [JsonPropertyName("fees")]
        public Fees Fees { get; set; }
        
        [JsonPropertyName("commitmentTerm")]
        public CommitmentTerm CommitmentTerm { get; set; }
    }

    private class Fees
    {
        [JsonPropertyName("recurringFee")]
        public decimal RecurringFee { get; set; }
        
        [JsonPropertyName("setupFee")]
        public decimal SetupFee { get; set; }
        
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    }

    private class CommitmentTerm
    {
        [JsonPropertyName("type")]
        public CommitmentLenght Type { get; set; }
        
        [JsonPropertyName("termEndDate")]
        public string TermEndDate { get; set; }
        
        [JsonPropertyName("renewalConfiguration")]
        public RenewalConfiguration RenewalConfiguration { get; set; }
        
        [JsonPropertyName("committedMinimalQuantities")]
        public List<CommittedMinimalQuantity> CommittedMinimalQuantities { get; set; } = new();
    }

    private class RenewalConfiguration
    {
        [JsonPropertyName("renewalDate")]
        public string RenewalDate { get; set; }
        
        [JsonPropertyName("scheduledQuantity")]
        public int ScheduledQuantity { get; set; }
    }

    private class CommittedMinimalQuantity
    {
        [JsonPropertyName("committedUntil")]
        public string CommittedUntil { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}

