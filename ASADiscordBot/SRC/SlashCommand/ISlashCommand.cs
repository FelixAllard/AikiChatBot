using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;

namespace ASADiscordBot.SlashCommand;

/// <summary>
/// ISlashCommands needs to be implemented to create new commands.
/// There can only be 100 global command and 100 Guild command
/// </summary>
public interface ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get;}
    public IServiceProvider ServiceProvider { get; set; }
    
    public IHttpClientFactory HttpClientFactory { get; set; }
    /// <summary>
    /// Must be set!
    /// </summary>
    public PermissionLevel PermissionLevel { get; set; }

    /// <summary>
    /// Prepare the slash Command Builder so that it is easy to get it afterward,
    /// it can also do operations before anything happens
    /// </summary>
    /// <param name="serviceProvider">A Service provider instance that could be usefully in case you want to use the database for example</param>
    /// <returns>And operation Result saying wether it worked or not</returns>
    public Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null);
    /// <summary>
    /// Will handle whenever someone on discord makes a call to this specific slash Command.
    /// </summary>
    /// <remarks>
    /// Needs to be async, or you might block the main thread leading to only 1 command working at any time.
    /// </remarks>
    /// <param name="command">The command provided by discord.net which contains everything about</param>
    /// <param name="caller">The user who called the command</param>
    /// <returns>Doesn't need to return anything</returns>
    public Task HandleClientCall(SocketSlashCommand command, SocketUser caller);
    /// <summary>
    /// A non overridable command that simply returns the built SlashCommand Builder stored in here
    /// </summary>
    /// <returns>The Slash Command builder</returns>

    public SlashCommandBuilder GetBuilder()
    {
        return builder;
    }
}