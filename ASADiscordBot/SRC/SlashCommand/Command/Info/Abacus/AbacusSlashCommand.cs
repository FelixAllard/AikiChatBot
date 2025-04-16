using System.Text;
using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using ASADiscordBot.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Abacus;

public class AbacusSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "ask";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name)
                .WithDescription("Ask questions to the AIKI Chatbot and get answers easily")
                /*.AddOption(new SlashCommandOptionBuilder()
                    .WithName("create")
                    .WithDescription("Create a new conversation")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(
                        "question", 
                        ApplicationCommandOptionType.String, 
                        "The question for the Chatbot", 
                        isRequired: true
                    )
                )*/
                .AddOption(
                    "question", 
                    ApplicationCommandOptionType.String, 
                    "The question for the Chatbot", 
                    isRequired: true
                );
        
        HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command, SocketUser caller1)
    {
        var client = HttpClientFactory.CreateClient();
        client = HttpClientFormatter.BuildAikiDataBuilderHttpClient(client).Result;
        // First lets extract our variables
        var caller = command.User;

        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
            var identity = context.Identities.FirstOrDefault(x => x.DiscordUserId == command.User.Id);
            if (identity == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = caller.Id,
                    Username = caller.Username,
                    IsAdmin = false,
                    IsWhitelisted = false,
                    DateAdded = DateTime.Now,
                    Password = "Aiki_Temp7!"
                });
                context.SaveChanges();
                
                identity = context.Identities.FirstOrDefault(x=>x.DiscordUserId == caller.Id);
            }
            if (!identity.IsWhitelisted)
            {
                EmbedBuilder responseDiscord = new EmbedBuilder()
                    .WithAuthor(caller.ToString(), caller.GetAvatarUrl() ?? caller.GetDefaultAvatarUrl())
                    .WithTitle("Unauthorized Access")
                    .WithDescription("Please gain access by asking an admin for access")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp();
                    
                await command.RespondAsync(embed: responseDiscord.Build());
            }
        }

        var fieldName = command.Data.Options.First().Name;
        var getOrSet = command.Data.Options.First().Options.First().Name;
        
        // Since there is no value on a get command, we use the ? operator because "Options" can be null.

        switch (fieldName)
        {
            case "create":
                

                    await command.RespondWithFileAsync("Hello World : Create");
                break;
            case "question":
                var valueQuestion = command.Data.Options.First().Options.First().Value;
                await command.RespondWithFileAsync("Hello World : Question" + valueQuestion);
                break;
        }

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
