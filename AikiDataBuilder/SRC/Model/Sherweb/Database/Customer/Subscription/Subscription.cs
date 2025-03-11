using System.ComponentModel.DataAnnotations;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription;

public class Subscription
{
    [Key]
    private string Id { get; set; }
    private string ProductName { get; set; }
    private string Description { get; set; }
    private string Sku { get; set; }
    private int Quantity { get; set; }
    private string BillingCycle { get; set; }
    private string PurchaseDate { get; set; }
    private CommitmentTerm.CommitmentTerm CommitmentTerm { get; set; }
}