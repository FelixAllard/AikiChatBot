using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Platform;

/// <summary>
/// Part of Platform
/// </summary>
public class MeterUsage
{
    [Key]
    private string MeterId { get; set; }
    private int TotalQuantity { get; set; }
    private int ConsumedQuantity { get; set; }
    private int AvailableQuantity { get; set; }
}