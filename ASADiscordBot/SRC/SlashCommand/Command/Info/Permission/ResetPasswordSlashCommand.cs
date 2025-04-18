using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Permission;

public class ResetPasswordSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; } = true;
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "reset-password";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.LogIn;
    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Allows you to reset your password. YOU MUST BE LOGGED IN");

        HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        
        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command, SocketUser caller)
    {
        var modal = new ModalBuilder()
            .WithTitle("Enter Your New Password")
            .WithCustomId("password_reset_modal")
            .AddTextInput("New Password", "password_input", TextInputStyle.Short, placeholder: "Enter your password...", required: true)
            .Build();

        await command.RespondWithModalAsync(modal);
    }
}