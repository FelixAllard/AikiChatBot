using Discord.Net;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace ASADiscordBot.SlashCommand;

using System;
using System.Linq;
using System.Reflection;

public class SlashCommandManager
{
    public static SlashCommandManager Instance { get; } = new SlashCommandManager();

    private DiscordSocketClient client;
    private List<ISlashCommand> slashCommands= new List<ISlashCommand>();
    private IServiceProvider _serviceProvider;
    public static SlashCommandManager Init(DiscordSocketClient client, 
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
    public async Task SlashCommandHandler(SocketSlashCommand command){
        var foundFunction = slashCommands.FirstOrDefault(x => x.Name == command.Data.Name);
        SocketUser socketUser = command.User as SocketUser;
        
        await foundFunction.HandleClientCall(command, socketUser);
    }
}
