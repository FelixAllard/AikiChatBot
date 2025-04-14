using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info.Permission;

public class MyInfoSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "info";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(
            Name);
        builder.WithDescription("Gives you the info on the permissions you have with the bot");

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
        var guildUser = command.User;
        Identity user;
        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
            user = context.Identities.FirstOrDefault(x=>x.DiscordUserId == guildUser.Id);
            if (user == null)
            {
                context.Identities.Add(new Identity()
                {
                    DiscordUserId = guildUser.Id,
                    Username = guildUser.Username,
                    IsAdmin = false,
                    IsWhitelisted = false,
                    DateAdded = DateTime.Now,
                    Password = "Aiki_Temp7!"
                });
                context.SaveChanges();
                
                user = context.Identities.FirstOrDefault(x=>x.DiscordUserId == guildUser.Id);
            }
        

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                .WithTitle("Your Info")
                .WithDescription($"ID         : {user.Id}            \n" +
                                 $"DiscordId  : {user.DiscordUserId} \n" +
                                 $"Username   : {guildUser.Username} \n" +
                                 $"Admin      : {user.IsAdmin}       \n" +
                                 $"Whitelist  : {user.IsWhitelisted} \n\n" +
                                 $"Created on : {user.DateAdded}     ")
                .WithColor(Color.Green)
                .WithCurrentTimestamp();
            
            await command.RespondAsync(embed: embedBuiler.Build());
            context.SaveChanges();
        }
        
    }
}