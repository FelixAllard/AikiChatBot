using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.ReceivableCharges;

public class ReceivableCharges
{
    public int Id { get; set; }
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public List<Charge> Charges { get; set; } = new List<Charge>();
}
