using AikiDataBuilder.Model.Sherweb.Database.Enumerators;

namespace AikiDataBuilder.Model.Sherweb.Database;

/// <summary>
/// Part of Subscription
/// </summary>
public class CommitmentTerm
{
    private CommitmentLenght CommitmentLenght { get; set; }
    private string TermEndDate { get; set; }
    private RenewalConfiguration RenewalConfiguration { get; set; }
    private CommittedMinimalQuantities CommittedMinimalQuantities { get; set; }
}