using ASADiscordBot.Database;
using ASADiscordBot.InteractionHandle;
using ASADiscordBot.SlashCommand;
using Discord;
using Discord.WebSocket;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace ASADiscordBot;

public class Program
{
    private static DiscordSocketClient _client;
    private static IServiceProvider _serviceProvider;
    public static string DefaultPassword;
    public static ulong SuperUserId;
    static IServiceProvider CreateProvider()
    {
        //Discord Socket COnfigs
        var config = new DiscordSocketConfig()
        {
            HandlerTimeout = 100000
        };
        string defaultConnection = Environment.GetEnvironmentVariable("DefaultConnection");
        //We replace the base url if we are running in docker
        if(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_DOCKER") == "true")
            defaultConnection = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_DOCKER");
        
        
        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddHttpClient()
            // Add your DbContext here:
            .AddDbContext<ASADbContext>(options =>
                options.UseSqlServer(
                        // Read from env, or config file
                        defaultConnection,
                        sqlOptions => sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)
                    )
                    .EnableSensitiveDataLogging()
            );
        DefaultPassword = Environment.GetEnvironmentVariable("DEFAULT_PWD");  
        string? superUserIdStr = Environment.GetEnvironmentVariable("SUPER_USER_ID");

        SuperUserId = ulong.TryParse(superUserIdStr, out var parsedId) ? parsedId : 1058211207678537748;
        
        
        
        //...
        return collection.BuildServiceProvider();
    }
    /// <summary>
    /// Program head
    /// </summary>
    public static async Task Main()
    {
        if(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_DOCKER") != "true")
            Env.Load("../../../../.env");
        _serviceProvider = CreateProvider();
        //GOES TO THE ROOT OF THE SOLUTION
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        
        _client = new DiscordSocketClient(_serviceProvider.GetRequiredService<DiscordSocketConfig>());

        // We init our managers
        SlashCommandManager.Init(
            _client, 
            _serviceProvider, 
            _serviceProvider.GetRequiredService<IHttpClientFactory>()
        );
        InteractionManager.Init(
            _client,
            _serviceProvider,
            _serviceProvider.GetRequiredService<IHttpClientFactory>()
        );
        
        _client.Log += Log;
        _client.Ready += SlashCommandManager.Instance.Build();
        _client.Ready += InteractionManager.Instance.Build();
        _client.SlashCommandExecuted += SlashCommandManager.Instance.SlashCommandHandler;
        _client.InteractionCreated += InteractionManager.Instance.InteractionHandler;
        


        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
        
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}