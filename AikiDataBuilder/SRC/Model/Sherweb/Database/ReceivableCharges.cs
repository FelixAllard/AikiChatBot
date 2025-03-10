using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database;

public class ReceivableCharges
{
    [Key]
    private string PeriodFrom { get; set; }
    private string PeriodTo { get; set; }
}