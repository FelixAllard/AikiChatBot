using Discord.Commands;

namespace ASADiscordBot.Commands.Module;

// Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
// https://docs.discordnet.dev/guides/text_commands/intro.html
public class InfoModule: ModuleBase<SocketCommandContext>
{
    [Command("hi")]
    [Summary("Squares a number.")]
    public async Task SquareAsync(
        [Summary("The number to square.")] 
        int num)
    {
        // We can also access the channel from the Command Context.
        await Context.Channel.SendMessageAsync($"Hi! How may I help you today?");
    }
    
}