using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Permission;

public class ElevateUserSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "op";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        builder.WithDescription("Elevate a user so he can elevate other and access admin privileges");
        builder.AddOption("user", ApplicationCommandOptionType.User, "The user you want to elevate the roles",
            isRequired: true);
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
        var guildUser = (SocketUser)command.Data.Options.First().Value;
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
            var identity = context.Identities.FirstOrDefault(x => x.DiscordUserId == command.User.Id);

            if (identity == null || !identity.IsAdmin)
            {
                await command.RespondAsync(embed: new EmbedBuilder()
                    .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                    .WithTitle("Unauthorized User")
                    .WithDescription($"You are not allowed to use this command.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build());
            }

            
            var userToOp =  context.Identities.FirstOrDefault(x => x.DiscordUserId == guildUser.Id);
            if (userToOp == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = guildUser.Id,
                    Username = guildUser.Username,
                    IsAdmin = true,
                    IsWhitelisted = true,
                    Password = "Aiki_Temp7!"
                });
            }
            else
            {
                userToOp.IsAdmin = true;
            }
            context.SaveChanges();
        }

        
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Result for Elevation")
            .WithDescription($"Successfully Opped {guildUser.Username} to Admin")
            .WithColor(Color.Purple)
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuiler.Build());
    }
}