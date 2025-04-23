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
     /// <param name="serviceProvider">The service provider which will be used when we want to access the database or other services</param>
     /// <returns>Whether it worked</returns>
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

     /// <summary>
     /// The function that will be called in respond to the interaction being pressed
     /// </summary>
     /// <param name="interaction">The interaction</param>
     /// <param name="modal">The modal</param>
     /// <returns>Nothing, this is just an async task</returns>
    public Task RespondToForm(SocketInteraction interaction, SocketModal modal);

}