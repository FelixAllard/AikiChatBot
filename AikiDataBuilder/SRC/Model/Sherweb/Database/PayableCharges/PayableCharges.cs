namespace AikiDataBuilder.Model.Sherweb.Database.PayableCharges;

public class PayableCharge
{
    public string PeriodFrom { get; set; }
    public string PeriodTo { get; set; }
    public List<PayableChargeDetail> Charges { get; set; } = new List<PayableChargeDetail>();
}