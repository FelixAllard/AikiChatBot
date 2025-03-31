using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database;

/// <summary>
/// The Sherweb Model, This is used to create the DB Context and is ideally not directly used
/// </summary>
public class SherwebModel
{
    [Key]
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public List<string> Path { get; set; }
    
    public string? SuspendedOn { get; set; }
    public virtual List<PlatformUsage> Platform { get; set; }
    public virtual ReceivableCharges ReceivableCharges { get; set; }
    public virtual List<Subscription> Subscriptions { get; set; }
}
public class PlatformUsage
{
    [Key]
    public int Id { get; set; }
    public string PlatformId { get; set; }
    public virtual List<MeterUsage> MeterUsages { get; set; }
    public virtual PlatformDetails PlatformDetails { get; set; }
}

public class MeterUsage
{
    [Key]
    public int Id { get; set; }
    public string MeterId { get; set; }
    public decimal TotalQuantities { get; set; }
    public decimal ConsumedQuantities { get; set; }
    public decimal AvailableQuantities { get; set; }
    public int PlatformUsageId { get; set; }
    [ForeignKey("PlatformUsageId")]
    public virtual PlatformUsage PlatformUsage { get; set; }
}

public class PlatformDetails
{
    [Key]
    public int Id { get; set; }
    public string PlatformId { get; set; }
    public string Details { get; set; } // Store as JSON string
    public int PlatformUsageId { get; set; }
    [ForeignKey("PlatformUsageId")]
    public virtual PlatformUsage PlatformUsage { get; set; }
}

public class ReceivableCharges
{
    [Key]
    public int Id { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public virtual List<ReceivableCharge> Charges { get; set; }
    public string CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual SherwebModel Customer { get; set; }
}

public class ReceivableCharge
{
    [Key]
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ChargeId { get; set; }
    public string ChargeName { get; set; }
    public Setup ChargeType { get; set; }
    public OneTime BillingCycleType { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostPriceProrated { get; set; }
    public string Currency { get; set; }
    public bool IsProratable { get; set; }
    public int ReceivableChargesId { get; set; }
    [ForeignKey("ReceivableChargesId")]
    public virtual ReceivableCharges ReceivableCharges { get; set; }
}

public class Subscription
{
    [Key]
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string Sku { get; set; }
    public decimal Quantity { get; set; }
    public string BillingCycle { get; set; }
    public string PurchaseDate { get; set; }
    public virtual SubscriptionFees Fees { get; set; }
    public virtual CommitmentTerm CommitmentTerm { get; set; }
    public string CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual SherwebModel Customer { get; set; }
}

public class SubscriptionFees
{
    [Key]
    public int Id { get; set; }
    public decimal RecurringFee { get; set; }
    public decimal SetupFee { get; set; }
    public string Currency { get; set; }
    public string SubscriptionId { get; set; }
    [ForeignKey("SubscriptionId")]
    public virtual Subscription Subscription { get; set; }
}

public class CommitmentTerm
{
    [Key]
    public int Id { get; set; }
    public CommitmentLenght Type { get; set; }
    public string TermEndDate { get; set; }
    public virtual RenewalConfiguration RenewalConfiguration { get; set; }
    public virtual List<CommittedMinimalQuantity> CommittedMinimalQuantities { get; set; }
    public string SubscriptionId { get; set; }
    [ForeignKey("SubscriptionId")]
    public virtual Subscription Subscription { get; set; }
}

public class RenewalConfiguration
{
    [Key]
    public int Id { get; set; }
    public string RenewalDate { get; set; }
    public decimal ScheduledQuantity { get; set; }
    public int CommitmentTermId { get; set; }
    [ForeignKey("CommitmentTermId")]
    public virtual CommitmentTerm CommitmentTerm { get; set; }
}

public class CommittedMinimalQuantity
{
    [Key]
    public int Id { get; set; }
    public string CommittedUntil { get; set; }
    public decimal Quantity { get; set; }
    public int CommitmentTermId { get; set; }
    [ForeignKey("CommitmentTermId")]
    public virtual CommitmentTerm CommitmentTerm { get; set; }
}

public class Platform
{
    [Key]
    public string Id { get; set; }
    public virtual List<LocalizedName> Name { get; set; }
}

public class LocalizedName
{
    [Key]
    public int Id { get; set; }
    public string Culture { get; set; }
    public string Value { get; set; }
    public string PlatformId { get; set; }
    [ForeignKey("PlatformId")]
    public virtual Platform Platform { get; set; }
}

public class PayableCharges
{
    [Key]
    public int Id { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public virtual List<PayableCharge> Charges { get; set; }
}

public class PayableCharge
{
    [Key]
    public int Id { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string ChargeId { get; set; }
    public string ChargeName { get; set; }
    public Setup ChargeType { get; set; }
    public OneTime BillingCycleType { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public decimal Quantity { get; set; }
    public decimal ListPrice { get; set; }
    public decimal NetPrice { get; set; }
    public decimal NetPriceProrated { get; set; }
    public decimal SubTotal { get; set; }
    public string Currency { get; set; }
    public bool IsBilled { get; set; }
    public bool IsProratable { get; set; }
    public virtual List<Deduction> Deductions { get; set; }
    public virtual List<Fee> Fees { get; set; }
    public virtual Invoice Invoice { get; set; }
    public virtual List<Tax> Taxes { get; set; }
    public virtual List<Tag> Tags { get; set; }
    public int PayableChargesId { get; set; }
    [ForeignKey("PayableChargesId")]
    public virtual PayableCharges PayableCharges { get; set; }
}

public class Deduction
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public PromotionalMoney DeductionType { get; set; }
    public decimal Value { get; set; }
    public decimal UnitValue { get; set; }
    public decimal TotalValue { get; set; }
    public int PayableChargeId { get; set; }
    [ForeignKey("PayableChargeId")]
    public virtual PayableCharge PayableCharge { get; set; }
}

public class Fee
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal UnitValue { get; set; }
    public decimal TotalValue { get; set; }
    public bool IsTaxable { get; set; }
    public int PayableChargeId { get; set; }
    [ForeignKey("PayableChargeId")]
    public virtual PayableCharge PayableCharge { get; set; }
}

public class Invoice
{
    [Key]
    public int Id { get; set; }
    public string Number { get; set; }
    public string Date { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public int PayableChargeId { get; set; }
    [ForeignKey("PayableChargeId")]
    public virtual PayableCharge PayableCharge { get; set; }
}

public class Tax
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal AppliedRate { get; set; }
    public int PayableChargeId { get; set; }
    [ForeignKey("PayableChargeId")]
    public virtual PayableCharge PayableCharge { get; set; }
}

public class Tag
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public int PayableChargeId { get; set; }
    [ForeignKey("PayableChargeId")]
    public virtual PayableCharge PayableCharge { get; set; }
}