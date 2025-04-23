using System.Reflection;
using ASADiscordBot.SlashCommand;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace ASADiscordBot.InteractionHandle;

public class InteractionManager
{
    /// <summary>
    /// Singleton Class
    /// </summary>
    public static InteractionManager Instance { get; } = new InteractionManager();

    private DiscordSocketClient client;
    private List<IInteraction> interactions= new List<IInteraction>();
    private IServiceProvider _serviceProvider;
    /// <summary>
    /// This will act as a constructor
    /// </summary>
    /// <param name="client">The discord socket client created in the Main Class</param>
    /// <param name="services">The service provider for dependency injection</param>
    /// <param name="httpClientFactory">HttpClientFactory so we can create http workers fast and efficiently</param>
    /// <returns></returns>
    public static InteractionManager Init(
        DiscordSocketClient client, 
        IServiceProvider services,
        IHttpClientFactory httpClientFactory
    )
    {
        Instance.client = client;
        Instance._serviceProvider = services;
        return Instance;
    }
    
    
    public Func<Task> Build()
    {
        return async () =>
        {
            // 1. Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            // 2. Find all types that implement ISlashCommand
            var interactionFromIInteraction = assembly.GetTypes()
                .Where(t => typeof(IInteraction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            var buildTasks = new List<Task>();

            foreach (var type in interactionFromIInteraction)
            {
                var instance = Activator.CreateInstance(type) as IInteraction;
                if (instance == null)
                {
                    Console.WriteLine($"Could not create instance of {type.Name}");
                    continue;
                }
                interactions.Add(instance);
                buildTasks.Add(instance.Init(_serviceProvider)); // Start building immediately
            }

            await Task.WhenAll(buildTasks); // Await all builds
        };
    }
    /// <summary>
    /// This function will be called whenever an interaction is mad on discord
    /// </summary>
    /// <param name="socketInteraction">Interaction information</param>
    
    public async Task InteractionHandler(SocketInteraction socketInteraction){
        
        if (socketInteraction is SocketModal modal)
        {
            var integrationConcerned = interactions.FirstOrDefault(x => x.InteractionId == modal.Data.CustomId);

            if (integrationConcerned is null)
            {
                await socketInteraction.RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("The Form does not exist")
                    .WithDescription("You somehow stumbled on the void of discord, no such form handler exist meaning your form was not set properly!")
                    .WithColor(Color.Orange)
                    .Build()
                );
                return;
            }
            await integrationConcerned.RespondToForm(socketInteraction, modal);
        }
        
    }
}