using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database;

public class Platform
{
    [Key]
    private string platformId { get; set; }
    private string[] platformDetails { get; set; }
}