namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription.CommitmentTerm;

/// <summary>
/// Part of subscription in commitment Term
/// </summary>
public class RenewalConfiguration
{
    public int Id { get; set; }
    public string RenewalDate { get; set; }
    public int ScheduledQuantity { get; set; }
}
