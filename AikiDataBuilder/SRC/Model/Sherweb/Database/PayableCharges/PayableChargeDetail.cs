using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.PayableCharges;

public class PayableChargeDetail
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ChargeId { get; set; }
    public string ChargeName { get; set; }
    public string ChargeType { get; set; }
    public string BillingCycleType { get; set; }
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
    public List<Deduction> Deductions { get; set; } = new List<Deduction>();
    public List<Fee> Fees { get; set; } = new List<Fee>();
    public Invoice Invoice { get; set; }
    public List<Tax> Taxes { get; set; } = new List<Tax>();
    public List<Tag> Tags { get; set; } = new List<Tag>();
}