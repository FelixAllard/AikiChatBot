using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;

namespace ASADiscordBot.SlashCommand.Command.Info;

public class ElevateUserSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "op";
    public async Task<OperationResult<bool>> Init()
    {
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Elevate a user so he can elevate other and access admin privileges");
        builder.AddOption("user", ApplicationCommandOptionType.User, "The user you want to elevate the roles", isRequired: true);

        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command)
    {
        throw new NotImplementedException();
    }
}