using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription.CommitmentTerm;

/// <summary>
/// Part of Subscription
/// </summary>
public class CommitmentTerm
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string TermEndDate { get; set; }
    public RenewalConfiguration RenewalConfiguration { get; set; }
    public List<CommittedMinimalQuantity> CommittedMinimalQuantities { get; set; } = new List<CommittedMinimalQuantity>();
}
