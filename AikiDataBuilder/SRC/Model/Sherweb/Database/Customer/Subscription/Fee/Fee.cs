using Microsoft.EntityFrameworkCore;

namespace AikiDataBuilder.Model.Sherweb.Database.Customer.Subscription.Fee;

/// <summary>
/// Part of subscription
/// </summary>
[Keyless]
public class Fee
{
    private int RecurringFee { get; set; }
    private int SetupFee { get; set; }
    /// <summary>
    /// The online model uses a Enum, but I leave it as a string for more allowed types of currency
    /// </summary>
    private string Currency { get; set; }
}