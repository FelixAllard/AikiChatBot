using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Abacus;

public class RefreshDataSlashCommand: ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "refresh-data";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public PermissionLevel PermissionLevel { get; set; }
    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name).WithDescription("Refresh the data in the database and use it to reload the AI");
        
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
        
        
    }
}