using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Platform;

public class Platform
{
    public int Id { get; set; }
    public string PlatformId { get; set; }
    public List<MeterUsage> MeterUsages { get; set; } = new List<MeterUsage>();
    public PlatformDetails PlatformDetails { get; set; }
}