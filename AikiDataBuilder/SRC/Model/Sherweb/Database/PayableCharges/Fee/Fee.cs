namespace AikiDataBuilder.Model.Sherweb.Database.PayableCharges.Fee;

public class Fee
{
    private string Name { get; set; }
    private float UnitValue { get; set; }
    private float TotalValue { get; set; }
    private bool IsTaxable { get; set; }
}