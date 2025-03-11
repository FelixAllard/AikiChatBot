using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription;

public class Subscription
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public string BillingCycle { get; set; }
    public string PurchaseDate { get; set; }
    
    public Fee.Fee Fees { get; set; }
    public CommitmentTerm.CommitmentTerm CommitmentTerm { get; set; }
}
