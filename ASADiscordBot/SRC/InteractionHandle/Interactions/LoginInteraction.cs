using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.InteractionHandle.Interactions;

public class LoginInteraction : IInteraction
{
    public string InteractionId { get; } = "password_modal";
    public IServiceProvider ServiceProvider { get; set; }
    public async Task RespondToForm(SocketInteraction interaction, SocketModal modal)
    {
        string password = modal.Data.Components
            .FirstOrDefault(x => x.CustomId == "password_input")?.Value;

        if (string.IsNullOrWhiteSpace(password))
        {
            await modal.RespondAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Empty Expression")
                    .WithDescription("You did not enter anything for the password")
                    .WithColor(Color.Gold)
                    .Build(), 
                ephemeral: true);
            return;
        }

        using (var scope = ServiceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

            var identity = context.Identities
                .FirstOrDefault(x => x.Username == interaction.User.Username && x.Password == password);

            if (identity == null)
            {
                await modal.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Wrong Password")
                    .WithDescription("Your password is not matching with the password in our database")
                    .WithColor(Color.Gold)
                    .Build(), ephemeral: true);
                return;
            }

            identity.LastLogin = DateTime.Now;
            await context.SaveChangesAsync();

            await modal.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Successful login!")
                    .WithDescription("You can now access your granted permissions.")
                    .WithColor(Color.Green)
                    .Build(),
                ephemeral: true
            );
        }
    }

}