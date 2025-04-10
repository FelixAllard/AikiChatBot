using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;

namespace ASADiscordBot.SlashCommand;

public interface ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get;}
    public IServiceProvider ServiceProvider { get; set; }


    public Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null);
    public Task HandleClientCall(SocketSlashCommand command);

    public SlashCommandBuilder GetBuilder()
    {
        return builder;
    }
}