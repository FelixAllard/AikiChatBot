using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.Sherweb.Database;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using AikiDataBuilder.Utilities;
using AikiDataBuilder.Utilities.EnumConverter;
using Microsoft.EntityFrameworkCore;
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
    public GetSubscriptions(IHttpClientFactory clientFactory) : base(clientFactory)
    {
        Url = "https://api.sherweb.com/service-provider/v1/billing/subscriptions?customerId={{customerId}}";
    }
    /// <summary>
    /// Adds the content to the database
    /// </summary>
    /// <param name="jsonContent">The content to serialize and then to add to the database</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override async Task<OperationResult<string>> AddToDatabase(IServiceScope scope, string jsonContent)
    {
        
        var sherwebDbContext = scope.ServiceProvider.GetRequiredService<SherwebDbContext>();
        
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
            
            
            
            foreach (var subscription in subscriptions.Items)
            {
                // 1. Create basic subscription properties
                var newSubscription = new Subscription()
                {
                    
                    Id = subscription.Id,
                    ProductName = subscription.ProductName,
                    Description = subscription.Description,
                    Sku = subscription.Sku,
                    Quantity = subscription.Quantity,
                    BillingCycle = subscription.BillingCycle,
                    PurchaseDate = subscription.PurchaseDate
                };

// 2. Create and assign Fees
                var fees = new SubscriptionFees()
                {
                    Currency = subscription.Fees.Currency,
                    SetupFee = subscription.Fees.SetupFee,
                    RecurringFee = subscription.Fees.RecurringFee
                };
                newSubscription.Fees = fees;

// 3. Create RenewalConfiguration
                var renewalConfig = new RenewalConfigurationDb();

                if (subscription?.CommitmentTerm?.RenewalConfiguration != null)
                {
                    if (subscription.CommitmentTerm.RenewalConfiguration.RenewalDate != null)
                    {
                        renewalConfig.RenewalDate = subscription.CommitmentTerm.RenewalConfiguration.RenewalDate;
                    }

                    if (subscription.CommitmentTerm.RenewalConfiguration.ScheduledQuantity != null)
                    {
                        renewalConfig.ScheduledQuantity = subscription.CommitmentTerm.RenewalConfiguration.ScheduledQuantity;
                    }
                }



                // 4. Create CommitmentTerm with RenewalConfiguration
                                // 1. Create empty CommitmentTermDb
                var commitmentTerm = new CommitmentTermDb();

                // 2. Check and set Type
                try 
                {
                    if (subscription != null && subscription.CommitmentTerm != null)
                    {
                        commitmentTerm.Type = subscription.CommitmentTerm.Type;
                    }
                    else
                    {
                        Console.WriteLine("Warning: subscription or CommitmentTerm is null when setting Type");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting Type: " + ex.Message);
                }

                // 3. Check and set TermEndDate
                try 
                {
                    if (subscription?.CommitmentTerm != null)
                    {
                        commitmentTerm.TermEndDate = subscription.CommitmentTerm.TermEndDate;
                    }
                    else
                    {
                        Console.WriteLine("Warning: subscription or CommitmentTerm is null when setting TermEndDate");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting TermEndDate: " + ex.Message);
                }

                // 4. Check and set RenewalConfiguration
                try 
                {
                    if (renewalConfig != null)
                    {
                        commitmentTerm.RenewalConfiguration = renewalConfig;
                    }
                    else
                    {
                        Console.WriteLine("Warning: renewalConfig is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting RenewalConfiguration: " + ex.Message);
                }

                // 5. Initialize CommittedMinimalQuantities list
                try 
                {
                    commitmentTerm.CommittedMinimalQuantities = new List<AikiDataBuilder.Model.Sherweb.Database.CommittedMinimalQuantity>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error initializing CommittedMinimalQuantities: " + ex.Message);
                }

                // 6. Verify the object was created properly
                try 
                {
                    if (commitmentTerm == null)
                    {
                        Console.WriteLine("Error: commitmentTerm is null after creation");
                    }
                    else
                    {
                        Console.WriteLine("CommitmentTerm created successfully with:");
                        Console.WriteLine($"- Type: {commitmentTerm.Type}");
                        Console.WriteLine($"- TermEndDate: {commitmentTerm.TermEndDate}");
                        Console.WriteLine($"- RenewalConfiguration: {(commitmentTerm.RenewalConfiguration != null ? "Set" : "Null")}");
                        Console.WriteLine($"- CommittedMinimalQuantities: {(commitmentTerm.CommittedMinimalQuantities != null ? "Initialized" : "Null")}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during verification: " + ex.Message);
                }
                
                newSubscription.CommitmentTerm = commitmentTerm;

// 5. Add to database context
                // 1. Get the customer entity
                var customer = sherwebDbContext.Customers
                    .Include(c => c.Subscriptions) // Ensure subscriptions are loaded
                    .FirstOrDefault(c => c.Id == _queryParameters["{{customerId}}"]);

                if (customer == null)
                {
                    throw new Exception("Customer not found"); // Handle the error appropriately
                }

// 2. Assign commitment term to the new subscription
                newSubscription.CommitmentTerm = commitmentTerm;

// 3. Add new subscription to customer's subscriptions
                customer.Subscriptions.Add(newSubscription);

// 4. Save changes to database (persist the new subscription)
                sherwebDbContext.SaveChanges();

// 5. Add committed minimal quantities if applicable
                if (newSubscription.CommitmentTerm?.CommittedMinimalQuantities != null)
                {
                    foreach (var committedMinimalQuantity in newSubscription.CommitmentTerm.CommittedMinimalQuantities)
                    {
                        newSubscription.CommitmentTerm.CommittedMinimalQuantities.Add(
                            new AikiDataBuilder.Model.Sherweb.Database.CommittedMinimalQuantity()
                            {
                                Quantity = committedMinimalQuantity.Quantity,
                                CommittedUntil = committedMinimalQuantity.CommittedUntil,
                            }
                        );
                    }
                }

// 6. Save changes again for committed quantities
                sherwebDbContext.SaveChanges();

            }
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
            Message = exception==null?"Successfully Added to database":$"Failed to add to database :{exception.GetBaseException()}\n{exception.StackTrace}",
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

