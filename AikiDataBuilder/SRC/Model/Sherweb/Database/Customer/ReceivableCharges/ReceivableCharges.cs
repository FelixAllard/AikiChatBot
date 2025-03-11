using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.ReceivableCharges;

public class ReceivableCharges
{
    [Key]
    private string PeriodFrom { get; set; }
    private string PeriodTo { get; set; }
}