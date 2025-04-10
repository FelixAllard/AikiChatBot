using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info;

public class ElevateUserSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "op";
    public IServiceProvider ServiceProvider { get; set; }

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
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
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

            var userToOp =  context.Identities.FirstOrDefault(x => x.Id == guildUser.Id);
            if (userToOp == null)
            {
                context.Identities.Add(new Identity()
                {
                    Id = guildUser.Id,
                    Username = guildUser.Username,
                    IsAdmin = true,
                    IsWhitelisted = true,
                    Password = "Aiki_Temp7!"
                });
            }
            userToOp.IsAdmin = true;
        }

        
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Result for Elevation")
            .WithDescription($"Successfully Opped {guildUser.Nickname} to Admin")
            .WithColor(Color.Purple)
            .WithCurrentTimestamp();
        
        await command.RespondAsync(embed: embedBuiler.Build());
    }
}