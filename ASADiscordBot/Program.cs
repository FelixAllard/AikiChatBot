using ASADiscordBot.Database;
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
    
    static IServiceProvider CreateProvider()
    {
        //Discord Socket COnfigs
        var config = new DiscordSocketConfig()
        {
            
            //...
        };
        
        
        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            // Add your DbContext here:
            .AddDbContext<ASADbContext>(options =>
                options.UseSqlServer(
                        // Read from env, or config file
                        Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING"),
                        sqlOptions => sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null)
                    )
                    .EnableSensitiveDataLogging()
            );

        
        
        
        //...
        return collection.BuildServiceProvider();
    }
    
    public static async Task Main()
    {
        Env.Load("../../../../.env");
        _serviceProvider = CreateProvider();
        //GOES TO THE ROOT OF THE SOLUTION
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        
        _client = new DiscordSocketClient();
        SlashCommandManager.Init(_client);
        
        _client.Log += Log;
        _client.Ready += SlashCommandManager.Instance.Build();
        _client.SlashCommandExecuted += SlashCommandManager.Instance.SlashCommandHandler;
        


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