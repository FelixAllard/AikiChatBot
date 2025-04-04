using AikiDataBuilder.Database;
using AikiDataBuilder.Model.Sherweb.Database;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;
using Newtonsoft.Json;
using SubscriptionFees = Sherweb.Apis.ServiceProvider.Models.SubscriptionFees;

namespace AikiDataBuilder.Services.SherwebFetcher.Requests;

public class GetPayableCharges : Request
{
    public GetPayableCharges(IHttpClientFactory clientFactory) : base(clientFactory)
    {
        Url = "https://api.sherweb.com/distributor/v1/billing/payable-charges";
    }



    public override async Task<OperationResult<string>> AddToDatabase(IServiceScope scope, string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return OperationResult<string>.Fail("JSON content is null or empty.");
        }

        Root root;
        try
        {
            root = JsonConvert.DeserializeObject<Root>(jsonContent);
            if (root == null)
            {
                return OperationResult<string>.Fail("Deserialized object is null.");
            }
        }
        catch (JsonException jsonEx)
        {
            // Handles JSON syntax or structure issues
            return OperationResult<string>.Fail($"JSON deserialization error: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            // Handles any other exception
            return OperationResult<string>.Fail($"Unexpected error during deserialization: {ex.Message}");
        }

        try
        {
            // Use your scope to get the database context or required services
            using var dbContext = scope.ServiceProvider.GetRequiredService<SherwebDbContext>();
            var PayableCharges = new PayableCharges()
            {
                PeriodFrom = root.PeriodFrom,
                PeriodTo = root.PeriodTo,
                Charges = new List<PayableCharge>()
            };

            foreach (var varCharge in root.Charges)
            {
                List<AikiDataBuilder.Model.Sherweb.Database.Deduction> deductions = new List<AikiDataBuilder.Model.Sherweb.Database.Deduction>();
                
                foreach (var deduction in varCharge.Deductions)
                {
                    deductions.Add(
                        new AikiDataBuilder.Model.Sherweb.Database.Deduction()
                        {
                            Code = deduction.Code,
                            Name = deduction.Name,
                            DeductionType = deduction.DeductionType,
                            Value = deduction.Value,
                            UnitValue = deduction.UnitValue,
                            TotalValue = deduction.TotalValue
                        }
                    );
                }
                List<AikiDataBuilder.Model.Sherweb.Database.Fee> fees = new List<AikiDataBuilder.Model.Sherweb.Database.Fee>();
                foreach (var fee in varCharge.Fees)
                {
                    fees.Add(new AikiDataBuilder.Model.Sherweb.Database.Fee()
                    {
                        Name = fee.Name,
                        UnitValue = fee.UnitValue,
                        TotalValue = fee.TotalValue,
                        IsTaxable = fee.IsTaxable
                    });
                }

                AikiDataBuilder.Model.Sherweb.Database.Invoice invoice =
                    new AikiDataBuilder.Model.Sherweb.Database.Invoice()
                    {
                        Number = varCharge.Invoice.Number,
                        Date = varCharge.Invoice.Date,
                        PeriodFrom = varCharge.Invoice.PeriodFrom,
                        PeriodTo = varCharge.Invoice.PeriodTo,
                    };
                List<AikiDataBuilder.Model.Sherweb.Database.Tax> taxes = new List<AikiDataBuilder.Model.Sherweb.Database.Tax>();
                foreach (var tax in varCharge.Taxes)
                {
                    taxes.Add(new AikiDataBuilder.Model.Sherweb.Database.Tax()
                    {
                        Name = tax.Name,
                        AppliedRate = tax.AppliedRate
                    });
                }
                List<AikiDataBuilder.Model.Sherweb.Database.Tag> tags = new List<AikiDataBuilder.Model.Sherweb.Database.Tag>();
                foreach (var tag in varCharge.Tags)
                {
                    tags.Add(new AikiDataBuilder.Model.Sherweb.Database.Tag()
                    {
                        Name = tag.Name,
                        Value = tag.Value
                    });
                }
                
                PayableCharges.Charges.Add(new PayableCharge()
                {
                    ProductId = varCharge.ProductId,
                    ProductName = varCharge.ProductName,
                    Sku = varCharge.Sku,
                    ChargeId = varCharge.ChargeId,
                    ChargeName = varCharge.ChargeName,
                    ChargeType = varCharge.ChargeType,
                    BillingCycleType = varCharge.BillingCycleType,
                    PeriodFrom = varCharge.PeriodFrom,
                    PeriodTo = varCharge.PeriodTo,
                    Quantity = varCharge.Quantity,
                    ListPrice = varCharge.ListPrice,
                    NetPrice = varCharge.NetPrice,
                    NetPriceProrated = varCharge.NetPriceProrated,
                    SubTotal = varCharge.SubTotal,
                    Currency = varCharge.Currency,
                    IsBilled = varCharge.IsBilled,
                    IsProratable = varCharge.IsProratable,
                    Deductions = deductions,
                    Fees = fees,
                    //Be carefull, this one is NOT a list
                    Invoice = invoice,
                    Taxes = taxes,
                    Tags = tags
                });

            }
            dbContext.PayableCharges.AddRange(PayableCharges);
            dbContext.SaveChanges();
            
            return OperationResult<string>.Success("Data added to the database successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"Error while saving to the database: {ex.Message}");
        }
    }

    
    /// <summary>
    /// Serialization Root model
    /// </summary>
    private class Root
    {
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
        public List<Charge> Charges { get; set; }
    }
    private class Charge
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string ChargeId { get; set; }
        public string ChargeName { get; set; }
        public Setup ChargeType { get; set; }
        public OneTime BillingCycleType { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
        public int Quantity { get; set; }
        public decimal ListPrice { get; set; }
        public decimal NetPrice { get; set; }
        public decimal NetPriceProrated { get; set; }
        public decimal SubTotal { get; set; }
        public string Currency { get; set; }
        public bool IsBilled { get; set; }
        public bool IsProratable { get; set; }
        public List<Deduction> Deductions { get; set; }
        public List<Fee> Fees { get; set; }
        public Invoice Invoice { get; set; }
        public List<Tax> Taxes { get; set; }
        public List<Tag> Tags { get; set; }
    }
    private class Deduction
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public PromotionalMoney DeductionType { get; set; }
        public decimal Value { get; set; }
        public decimal UnitValue { get; set; }
        public decimal TotalValue { get; set; }
    }
    private class Fee
    {
        public string Name { get; set; }
        public decimal UnitValue { get; set; }
        public decimal TotalValue { get; set; }
        public bool IsTaxable { get; set; }
    }
    private class Invoice
    {
        public string Number { get; set; }
        public string Date { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo { get; set; }
    }
    private class Tax
    {
        public string Name { get; set; }
        public decimal AppliedRate { get; set; }
    }
    private class Tag
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}