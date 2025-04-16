using Discord.Net;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace ASADiscordBot.SlashCommand;

using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Responsible for handling slash commands
/// </summary>
public class SlashCommandManager
{
    /// <summary>
    /// Singleton Class
    /// </summary>
    public static SlashCommandManager Instance { get; } = new SlashCommandManager();

    private DiscordSocketClient client;
    private List<ISlashCommand> slashCommands= new List<ISlashCommand>();
    private IServiceProvider _serviceProvider;
    /// <summary>
    /// This will act as a constructor
    /// </summary>
    /// <param name="client">The discord socket client created in the Main Class</param>
    /// <param name="services">The service provider for dependency injection</param>
    /// <param name="httpClientFactory">HttpClientFactory so we can create http workers fast and efficiently</param>
    /// <returns></returns>
    public static SlashCommandManager Init(
        DiscordSocketClient client, 
        IServiceProvider services,
        IHttpClientFactory httpClientFactory
    )
    {
        Instance.client = client;
        Instance._serviceProvider = services;
        return Instance;
    }
    /// <summary>
    /// Creates all the slash commands and register them on discord!
    /// </summary>
    /// <returns>Func Task which is the necessary return type since we want some async process while allowing the system multithreading</returns>
    public Func<Task> Build()
    {
        return async () =>
        {
            // 1. Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            // 2. Find all types that implement ISlashCommand
            var slashCommandTypes = assembly.GetTypes()
                .Where(t => typeof(ISlashCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            var buildTasks = new List<Task>();

            foreach (var type in slashCommandTypes)
            {
                var instance = Activator.CreateInstance(type) as ISlashCommand;
                if (instance == null)
                {
                    Console.WriteLine($"Could not create instance of {type.Name}");
                    continue;
                }
                slashCommands.Add(instance);
                buildTasks.Add(instance.Init(_serviceProvider)); // Start building immediately
            }

            await Task.WhenAll(buildTasks); // Await all builds
        
            var addingTasks = new List<Task>();
            foreach (var instance in slashCommands)
            {
                try
                {
                    addingTasks.Add(client.CreateGlobalApplicationCommandAsync(instance.GetBuilder().Build()));
                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                    Console.WriteLine(json);
                }
            }

            await Task.WhenAll(addingTasks); // Await all adding tasks too!
        };
    }
    /// <summary>
    /// Will get called whenever a slash command is executed and will redirect to the right endpoint
    /// </summary>
    /// <param name="command">The command provided by the Discord API</param>
    public async Task SlashCommandHandler(SocketSlashCommand command){
        var foundFunction = slashCommands.FirstOrDefault(x => x.Name == command.Data.Name);
        SocketUser socketUser = command.User as SocketUser;
        
        await foundFunction.HandleClientCall(command, socketUser);
    }
}
