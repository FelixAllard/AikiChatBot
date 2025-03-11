using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Platform;

/// <summary>
/// Part of Platform
/// </summary>
public class MeterUsage
{
    public int Id { get; set; }
    public string MeterId { get; set; }
    public int TotalQuantities { get; set; }
    public int ConsumedQuantities { get; set; }
    public int AvailableQuantities { get; set; }
}