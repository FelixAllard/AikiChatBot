using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.PayableCharges.Deduction;

public class Deduction
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string DeductionType { get; set; }
    public decimal Value { get; set; }
    public decimal UnitValue { get; set; }
    public decimal TotalValue { get; set; }
}