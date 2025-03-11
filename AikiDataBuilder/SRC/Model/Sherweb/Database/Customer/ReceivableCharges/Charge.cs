using System.ComponentModel.DataAnnotations;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.ReceivableCharges;

/// <summary>
/// A product is constituted of one or multiple charges.
/// </summary>
public class Charge
{
    [Key]
    private string ChargeId { get; set; }
    private string ProductName { get; set; }
    private string Sku { get; set; }
    private string ChargeName { get; set; }
    private Setup ChargeType { get; set; }
    private OneTime BillingCycleType { get; set; }
    private string PeriodFrom { get; set; }
    private string PeriodTo { get; set; }
    private int Quantity { get; set; }
    private int CostPrice { get; set; }
    private int CostPriceProrated { get; set; }
    private string Currency { get; set; }
    private bool isProrated { get; set; }
}