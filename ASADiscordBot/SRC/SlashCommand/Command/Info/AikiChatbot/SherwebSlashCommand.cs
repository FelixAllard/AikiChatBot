using System.Text;
using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using ASADiscordBot.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.AikiChatbot;

public class SherwebSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "sherweb";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Access Aiki Sherweb Data Builder");
        builder.AddOption(new SlashCommandOptionBuilder()
            .WithName("information")
            .WithDescription("Gets or sets the field A")
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("customer")
                .WithDescription("Get the Customers Information from the database in json format")
                .WithType(ApplicationCommandOptionType.SubCommand)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("payable-charges")
                .WithDescription("Get the Payable Charges Information from the database in json format")
                .WithType(ApplicationCommandOptionType.SubCommand)
            )
        );
        
        HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command)
    {
        var client = HttpClientFactory.CreateClient();
        client = HttpClientFormatter.BuildAikiDataBuilderHttpClient(client).Result;
        // First lets extract our variables
        var caller = command.User;
        var fieldName = command.Data.Options.First().Name;
        var getOrSet = command.Data.Options.First().Options.First().Name;
        
        // Since there is no value on a get command, we use the ? operator because "Options" can be null.

        switch (fieldName)
        {
            case "information":
            {
                if(getOrSet == "customer")
                {
                    var response = client.Send(new HttpRequestMessage(HttpMethod.Get, "/customers/format"));
                    response.EnsureSuccessStatusCode();
                    var content = response.Content;
                    
                    var json = content.ReadAsStringAsync().Result;
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    using var stream = new MemoryStream(jsonBytes);
                    
                    
                    EmbedBuilder responseDiscord = new EmbedBuilder()
                        .WithAuthor(caller.ToString(), caller.GetAvatarUrl() ?? caller.GetDefaultAvatarUrl())
                        .WithTitle("Success")
                        .WithDescription("You will find all the customers attached to this message!")
                        .WithColor(Color.Green)
                        .WithCurrentTimestamp();
                    
                    await command.RespondWithFileAsync(stream, "customers.json", text: "Here's the customers data as JSON.", embed: responseDiscord.Build());
                }
                else if (getOrSet == "payable-charges")
                {
                    var response = client.Send(new HttpRequestMessage(HttpMethod.Get, "/payable-charges/format"));
                    response.EnsureSuccessStatusCode();
                    var content = response.Content;
                    
                    var json = content.ReadAsStringAsync().Result;
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    using var stream = new MemoryStream(jsonBytes);
                    
                    EmbedBuilder responseDiscord = new EmbedBuilder()
                        .WithAuthor(caller.ToString(), caller.GetAvatarUrl() ?? caller.GetDefaultAvatarUrl())
                        .WithTitle("Success")
                        .WithDescription("You will find all the payable-charges attached to this message!")
                        .WithColor(Color.Green)
                        .WithCurrentTimestamp();
                    
                    await command.RespondWithFileAsync(stream, "payable_charges.json", text: "Here's the customer data as JSON.", embed: responseDiscord.Build());
                }
            }
                break;
            /*case "field-b":
            {
                if (getOrSet == "get")
                {
                    await command.RespondAsync($"The value of `field-b` is `{FieldB}`");
                }
                else if (getOrSet == "set")
                {
                    this.FieldB = (int)value;
                    await command.RespondAsync($"`field-b` has been set to `{FieldB}`");
                }
            }
                break;
            case "field-c":
            {
                if (getOrSet == "get")
                {
                    await command.RespondAsync($"The value of `field-c` is `{FieldC}`");
                }
                else if (getOrSet == "set")
                {
                    this.FieldC = (bool)value;
                    await command.RespondAsync($"`field-c` has been set to `{FieldC}`");
                }
            }
                break;*/
        }
    }
}