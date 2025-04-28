using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using ASADiscordBot.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.AikiChatbot;

public class RefreshDatabaseDataSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; } = true;
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "refresh-database-data";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Admin;
    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Reresh the database with the different APIs");
        
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
        await command.DeferAsync();
        var client = HttpClientFactory.CreateClient();
        client = HttpClientFormatter.BuildAikiDataBuilderHttpClient(client).Result;

        var result = client.Send(new HttpRequestMessage(HttpMethod.Get, "api/data-builder"));

        if (!result.IsSuccessStatusCode)
        {
            await command.FollowupAsync( embed :new EmbedBuilder()
                .WithTitle("Unable to refresh data")
                .WithDescription($"The server responded with {result.StatusCode}, unable to refresh the data")
                .WithColor(Color.Red)
                .Build()
            );
            return;
        }
        await command.FollowupAsync(embed: new EmbedBuilder()
            .WithTitle("Data refreshed!")
            .WithDescription($"The new data has successfully been copied to the database")
            .WithColor(Color.Green)
            .Build()
        );
    }
}