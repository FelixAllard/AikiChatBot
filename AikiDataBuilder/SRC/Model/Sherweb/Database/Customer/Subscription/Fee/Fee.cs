using Microsoft.EntityFrameworkCore;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription.Fee;

/// <summary>
/// Part of subscription
/// </summary>
[Keyless]
public class Fee
{
    public int Id { get; set; }
    public decimal RecurringFee { get; set; }
    public decimal SetupFee { get; set; }
    public string Currency { get; set; }
}
