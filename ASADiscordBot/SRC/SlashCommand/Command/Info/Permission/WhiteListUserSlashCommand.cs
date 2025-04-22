using System.Diagnostics;
using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Permission;

public class WhiteListUserSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "whitelist";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Admin;

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Whitelist a user so he has access to the discord bot");
        builder.AddOption(new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Add a new user to the discord bot")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(
                "user", 
                ApplicationCommandOptionType.User, 
                "The user you want to give access to the discord bot", 
                isRequired: true
            )
        );
        builder.AddOption(new SlashCommandOptionBuilder()
            .WithName("remove")
            .WithDescription("Remove a user access to the discord bot")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(
                "user", 
                ApplicationCommandOptionType.User, 
                "The user you want to remove access to the discord bot", 
                isRequired: true
            )
        );
        builder.AddOption(new SlashCommandOptionBuilder()
            .WithName("list")
            .WithDescription("List all users who have access to the discord bot")
            .WithType(ApplicationCommandOptionType.SubCommand)
        );
        
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
        var taskName = (string)command.Data.Options.First().Name;
        
        
        switch (taskName)
        {
            case "add":
                var user1 = (SocketUser)command.Data.Options.First().Options.First().Value;
                await AddUser(command, user1);
                break;
            case "remove":
                var user2 = (SocketUser)command.Data.Options.First().Options.First().Value;
                await RemoveUser(command,user2);
                break;
            
            case "list":
                await ListUsers(command);
                break;
            
            default:
                await command.RespondAsync(embed:new EmbedBuilder()
                    .WithTitle("Whitelist command not found")
                    .WithDescription("Please use a valid subcommand")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .WithAuthor(command.User.Username,command.User.GetAvatarUrl())
                    .Build()
                );
                break;
            
        }
        
    }

    public async Task AddUser(SocketSlashCommand command,SocketUser user)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

            
            var userToOp =  context.Identities.FirstOrDefault(x => x.DiscordUserId == user.Id);
            if (userToOp == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = user.Id,
                    Username = user.Username,
                    IsAdmin = false,
                    IsWhitelisted = true,
                    Password = "Aiki_Temp7!"
                });
            }
            else
            {
                userToOp.IsWhitelisted = true;
            }
            context.SaveChanges();
        }

        
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(user.ToString(), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithTitle("Result for Whitelisting")
            .WithDescription($"Successfully Whitelisted {user.Username} !!!")
            .WithColor(Color.Purple)
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuiler.Build());
        
    }

    public async Task RemoveUser(SocketSlashCommand command,SocketUser user)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
            
            var userToOp =  context.Identities.FirstOrDefault(x => x.DiscordUserId == user.Id);
            if (userToOp == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = user.Id,
                    Username = user.Username,
                    IsAdmin = false,
                    IsWhitelisted = false,
                    Password = "Aiki_Temp7!"
                });
            }
            else
            {
                userToOp.IsWhitelisted = false;
            }
            context.SaveChanges();
        }

        
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(user.ToString(), user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithTitle("Result for Whitelisting")
            .WithDescription($"Successfully removed {user.Username} from whitelist !!!")
            .WithColor(Color.Purple)
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuiler.Build());
        
    }

    public async Task ListUsers(SocketSlashCommand command)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

            var users = context.Identities.Where(x => x.IsWhitelisted || x.IsAdmin || x.IsSuperAdmin);

            string allUsers = string.Empty;
            foreach (var user in users)
            {
                allUsers += $"{user.Username}\n";
            }

            var embed = new EmbedBuilder()
                .WithTitle("All Whitelisted Users")
                .WithDescription(allUsers)
                .WithColor(Color.Green)
                .WithAuthor(command.User.Username, command.User.GetAvatarUrl())
                .WithCurrentTimestamp();
            
            await command.RespondAsync(embed: embed.Build(), ephemeral: true);

        }
    }
}