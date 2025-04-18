namespace ASADiscordBot.Model;

/// <summary>
/// Any permissionlevel gives access to all lower permission level
/// </summary>
public enum PermissionLevel
{
    /// <summary>
    /// Anyone
    /// </summary>
    Open,
    /// <summary>
    /// Can be anyone, but must be Login
    /// </summary>
    LogIn,
    /// <summary>
    /// Those Whitelisted
    /// </summary>
    Listed,
    /// <summary>
    /// Administrator
    /// </summary>
    Admin,
    /// <summary>
    /// Super admin which will be the boss ( Francesco )
    /// </summary>
    SuperAdmin
}