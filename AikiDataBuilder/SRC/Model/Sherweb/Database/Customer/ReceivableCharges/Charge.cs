using System.ComponentModel.DataAnnotations;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.ReceivableCharges;

/// <summary>
/// A product is constituted of one or multiple charges.
/// </summary>
public class Charge
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ChargeId { get; set; }
    public string ChargeName { get; set; }
    public string ChargeType { get; set; }
    public string BillingCycleType { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPriceProrated { get; set; }
    public string Currency { get; set; }
    public bool IsProratable { get; set; }
}
