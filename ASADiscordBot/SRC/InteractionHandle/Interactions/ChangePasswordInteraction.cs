using ASADiscordBot.Database;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.InteractionHandle.Interactions;

public class ChangePasswordInteraction : IInteraction
{
    public string InteractionId { get; } = "password_reset_modal";
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
            

            identity.Password = password;
            await context.SaveChangesAsync();

            await modal.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("New Password Set!")
                    .WithDescription("Your new password has been set!")
                    .WithColor(Color.Green)
                    .Build(),
                ephemeral: true
            );
        }
    }
}