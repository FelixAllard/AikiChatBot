using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.PayableCharges;

public class Charge
{
    private string ProductId {get; set;}
    private string ProductName {get; set;}
    private string Sku {get; set;}
    private string ChargeId {get; set;}
    private string ChargeName {get; set;}
    private Setup ChargeType {get; set;}
    private OneTime BillingCycleType {get; set;}
    private string PeriodFrom {get; set;}
    private string PeriodTo {get; set;}
    private int Quantity {get; set;}
    private int ListPrice {get; set;}
    private int NetPrice {get; set;}
    private int NetPriceProrated {get; set;}
    private int SubTotal {get; set;}
    private string Currency {get; set;}
    private bool IsBilled {get; set;}
    private bool IsProratable {get; set;}
}