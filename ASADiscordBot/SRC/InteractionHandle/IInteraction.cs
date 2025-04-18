using ASADiscordBot.Framework;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.InteractionHandle;

public interface IInteraction
{
    
    public string InteractionId { get; }
    public IServiceProvider ServiceProvider { get; set; }
 /// <summary>
 /// Let's provide a base implementation
 /// </summary>
 /// <param name="serviceProvider"></param>
 /// <returns></returns>
    public virtual Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;

        return Task.FromResult(new OperationResult<bool>()
        {
            IsSuccess = true,
            Result = true,
            Message = "Success"
        });
    }

    public Task RespondToForm(SocketInteraction interaction, SocketModal modal);

}