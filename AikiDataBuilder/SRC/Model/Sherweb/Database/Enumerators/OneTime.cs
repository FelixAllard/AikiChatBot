namespace AikiDataBuilder.Model.Sherweb.Database.Enumerators;

/// <summary>
/// Enumerator used my Charge, for variable ReceivableBilingCycleType
/// </summary>
public enum OneTime
{
    /// <summary>
    /// charged once, with no recurrence
    /// </summary>
    OneTime,
    /// <summary>
    /// charged monthly
    /// </summary>
    Monthly,
    /// <summary>
    /// charged yearly
    /// </summary>
    Yearly
}