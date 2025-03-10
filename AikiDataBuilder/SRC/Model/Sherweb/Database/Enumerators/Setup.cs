namespace AikiDataBuilder.Model.Sherweb.Database.Enumerators;

/// <summary>
/// Enumerator used by the variable called ChargeType in the Charge Model class
/// </summary>
public enum Setup
{
    /// <summary>
    /// Occurs only once and can be used for activation, cancellation or setup fees
    /// </summary>
    Setup,
    /// <summary>
    /// Invoiced on a monthly or yearly basis
    /// </summary>
    Recurring,
    /// <summary>
    /// Varies based on the quantity of the product or service consumed by the customer
    /// </summary>
    Usage,
    /// <summary>
    /// Charge type not found. This may happen when querying data from older invoices
    /// </summary>
    Unknown
}