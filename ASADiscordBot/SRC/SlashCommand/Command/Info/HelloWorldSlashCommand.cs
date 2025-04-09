using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;

namespace ASADiscordBot.SlashCommand.Command.Info;

public class HelloWorldSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "hello-world";

    public async Task<OperationResult<bool>> Init()
    {
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Simple test command to make sure the application is working");

        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command)
    {
        var guildUser = command.User ;
        List<string> differentHello = new List<string>()
        {
            $"Hey {guildUser.Username}!",
            $"Hi {guildUser.Username}, how can I help you?",
            $"Hello World! {guildUser.Username}!"
        };
        var roleList = "";
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(differentHello[new Random().Next(0, differentHello.Count)])
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        
        var builder = new ComponentBuilder()
            .WithButton("label", "custom-id");
        
        await command.RespondAsync(embed: embedBuiler.Build(), components: builder.Build());
    }
}