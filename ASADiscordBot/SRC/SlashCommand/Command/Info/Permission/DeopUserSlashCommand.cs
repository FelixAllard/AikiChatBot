using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Permission;

public class DeopUserSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "deop";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.SuperAdmin;

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Deop a user so he will loose Admin Privileges");
        builder.AddOption("user", ApplicationCommandOptionType.User, "The user you want to deop", isRequired: true);
        
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
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
        using (var scope = ServiceProvider.CreateScope())
        {
            
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

            var userToOp =  context.Identities.FirstOrDefault(x => x.DiscordUserId == guildUser.Id);
            if (userToOp == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = guildUser.Id,
                    Username = guildUser.Username,
                    IsAdmin = false,
                    IsWhitelisted = true,
                    Password = "Aiki_Temp7!"
                });
            }
            else
            {
                userToOp.IsAdmin = false;
            }
            context.SaveChanges();
            
        }

        
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Result for Deop")
            .WithDescription($"Removed {guildUser.Nickname} from Admins")
            .WithColor(Color.Purple)
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuiler.Build());
    }
}