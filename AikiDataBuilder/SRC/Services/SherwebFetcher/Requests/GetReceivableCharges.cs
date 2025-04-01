using System.Text.Json;
using System.Text.Json.Serialization;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.Sherweb.Database;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using AikiDataBuilder.Utilities;
using Sherweb.Apis.ServiceProvider.Models;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using ReceivableCharges = AikiDataBuilder.Model.Sherweb.Database.ReceivableCharges;

namespace AikiDataBuilder.Services.SherwebFetcher.Requests;

/// <summary>
/// Get all Request :) First request done and might be modified
/// </summary>
public class GetReceivableCharges : Request
{
    public GetReceivableCharges(HttpClient clientFactory, SherwebDbContext sherwebDBContext) : base(clientFactory, sherwebDBContext)
    {
        Url = "https://api.sherweb.com/service-provider/v1/billing/receivable-charges?customerId={{customerId}}";
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

            ChargeModel? chargeModel = JsonSerializer.Deserialize<ChargeModel>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (chargeModel == null)
            {
                throw new InvalidOperationException("Failed to deserialize the JSON content into the expected structure.");
            }
            
            // Drop all existing charges
            _sherwebDBContext.ReceivableCharge.RemoveRange(_sherwebDBContext.ReceivableCharge);
            _sherwebDBContext.ReceivableCharges.RemoveRange(_sherwebDBContext.ReceivableCharges);


            var receivableCharges = _sherwebDBContext.ReceivableCharges.Add(new ReceivableCharges()
            {
                //Id Is incremental
                PeriodFrom = chargeModel.PeriodFrom,
                PeriodTo = chargeModel.PeriodTo,
            });
            
            
            foreach (var charge in chargeModel.Charges)
            {
                // Add new charge
                receivableCharges.Entity.Charges.Add(new ReceivableCharge()
                {
                    ProductName = charge.ProductName,
                    Sku = charge.Sku,
                    ChargeId = charge.ChargeId,
                    ChargeName = charge.ChargeName,
                    ChargeType = charge.ChargeType,
                    BillingCycleType = charge.BillingCycleType,
                    PeriodFrom = charge.PeriodFrom,
                    PeriodTo = charge.PeriodTo,
                    Quantity = charge.Quantity,
                    CostPrice = charge.CostPrice,
                    CostPriceProrated = charge.CostPriceProrated,
                    Currency = charge.Currency,
                    IsProratable = charge.IsProratable
                });
            }

            await _sherwebDBContext.SaveChangesAsync(); // Ensure database changes are saved
        }
        catch (JsonException jsonEx)
        {
            Console.Error.WriteLine($"Error deserializing JSON: {jsonEx.Message}");
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
            Message = "Success in adding to database",
            Exception = exception,
            Result = jsonContent
        };
    }
    //Here are the serialization classes

    /// <summary>
    /// Main Class
    /// </summary>
    private class ChargeModel
    {
        [JsonPropertyName("periodFrom")]
        public string PeriodFrom { get; set; }
    
        [JsonPropertyName("periodTo")]
        public string PeriodTo { get; set; }
    
        [JsonPropertyName("charges")]
        public List<ChargeDetail> Charges { get; set; }
    }
    /// <summary>
    /// Part of ChargeModel
    /// </summary>

    private class ChargeDetail
    {
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
    
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
    
        [JsonPropertyName("chargeId")]
        public string ChargeId { get; set; }
    
        [JsonPropertyName("chargeName")]
        public string ChargeName { get; set; }
    
        [JsonPropertyName("chargeType")]
        public Setup ChargeType { get; set; }
    
        [JsonPropertyName("billingCycleType")]
        public OneTime BillingCycleType { get; set; }
    
        [JsonPropertyName("periodFrom")]
        public string PeriodFrom { get; set; }
    
        [JsonPropertyName("periodTo")]
        public string PeriodTo { get; set; }
    
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    
        [JsonPropertyName("costPrice")]
        public decimal CostPrice { get; set; }
    
        [JsonPropertyName("costPriceProrated")]
        public decimal CostPriceProrated { get; set; }
    
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
    
        [JsonPropertyName("isProratable")]
        public bool IsProratable { get; set; }
    }

}

