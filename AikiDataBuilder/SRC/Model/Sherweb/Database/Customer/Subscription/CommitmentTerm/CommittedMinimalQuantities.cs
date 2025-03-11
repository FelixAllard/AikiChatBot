namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription.CommitmentTerm;

/// <summary>
/// Part of subscription in commitment Term
/// </summary>
public class CommittedMinimalQuantity
{
    public int Id { get; set; }
    public string CommittedUntil { get; set; }
    public int Quantity { get; set; }
}
