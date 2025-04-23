using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Model;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
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
        // Find the matching function name
        var foundFunction = slashCommands.FirstOrDefault(x => x.Name == command.Data.Name);
        
        // Find the user that called the command
        SocketUser socketUser = command.User as SocketUser;
        
        //Find the same user, from our database, and if it doesn't exist we create it
        Identity user;
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
            user = context.Identities.FirstOrDefault(x => x.DiscordUserId == socketUser.Id);
            if (user == null)
            {
                user = context.Identities.Add(new Identity()
                {
                    DiscordUserId = socketUser.Id,
                    Username = socketUser.Username,
                    IsAdmin = false,
                    IsWhitelisted = false,
                    DateAdded = DateTime.Now,
                    Password = "Aiki_Temp7!"
                }).Entity;
                // If it is the defined superuser, we gave it superuser rights
                if (socketUser.Id == Program.SuperUserId)
                {
                    user.IsSuperAdmin = true;
                }
                context.SaveChanges();
            }
            
        }
        // We use the permission level and see what we do depending on it
        switch (foundFunction.PermissionLevel)
        {
            // Open is for anybody
            case (PermissionLevel.Open):
                await foundFunction.HandleClientCall(command, socketUser);
                break;
            // Login requires to be logged in
            case (PermissionLevel.LogIn):
                // If it's been more than a day, then require a new login
                if (user.LastLogin.AddDays(1) <= DateTime.Now)
                {
                    await command.RespondAsync(embed: new EmbedBuilder()
                        .WithAuthor(socketUser.ToString(),
                            socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                        .WithTitle("Not Logged In")
                        .WithDescription($"You need to be Logged in to make this command! Please Login using /login")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build()
                    );
                    break;
                }

                await foundFunction.HandleClientCall(command, socketUser);

                break;
            // Need to have been whitelisted
            case (PermissionLevel.Listed):


                if (user.IsWhitelisted || user.IsAdmin || user.IsSuperAdmin)
                {
                    if (user.LastLogin.AddDays(1) <= DateTime.Now)
                    {
                        await command.RespondAsync(embed: new EmbedBuilder()
                            .WithAuthor(socketUser.ToString(),
                                socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                            .WithTitle("Not Logged In")
                            .WithDescription(
                                $"You need to be Logged in to make this command! Please Login using /login")
                            .WithColor(Color.Red)
                            .WithCurrentTimestamp()
                            .Build()
                        );
                        break;
                    }

                    await foundFunction.HandleClientCall(command, socketUser);
                }

                else
                    await command.RespondAsync(embed: new EmbedBuilder()
                        .WithAuthor(socketUser.ToString(),
                            socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                        .WithTitle("Unauthorized User")
                        .WithDescription($"You are not allowed to use this command. You need to be whitelisted!")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build()
                    );

                break;
            // Needs to be an admin
            case (PermissionLevel.Admin):
                if (user.IsAdmin || user.IsSuperAdmin)
                {
                    if (user.LastLogin.AddDays(1) <= DateTime.Now)
                    {
                        await command.RespondAsync(embed: new EmbedBuilder()
                            .WithAuthor(socketUser.ToString(),
                                socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                            .WithTitle("Not Logged In")
                            .WithDescription(
                                $"You need to be Logged in to make this command! Please Login using /login")
                            .WithColor(Color.Red)
                            .WithCurrentTimestamp()
                            .Build()
                        );
                        break;
                    }

                    await foundFunction.HandleClientCall(command, socketUser);
                }

                else
                    await command.RespondAsync(embed: new EmbedBuilder()
                        .WithAuthor(socketUser.ToString(),
                            socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                        .WithTitle("Unauthorized User")
                        .WithDescription($"You are not allowed to use this command. You need to be an admin!")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build()
                    );

                break;
            // Needs to be the superadmin
            case (PermissionLevel.SuperAdmin):
                if (user.IsSuperAdmin)
                {
                    if (user.LastLogin.AddDays(1) <= DateTime.Now)
                    {
                        await command.RespondAsync(embed: new EmbedBuilder()
                            .WithAuthor(socketUser.ToString(),
                                socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                            .WithTitle("Not Logged In")
                            .WithDescription(
                                $"You need to be Logged in to make this command! Please Login using /login")
                            .WithColor(Color.Red)
                            .WithCurrentTimestamp()
                            .Build()
                        );
                        break;
                    }

                    await foundFunction.HandleClientCall(command, socketUser);
                }

                else
                    await command.RespondAsync(embed: new EmbedBuilder()
                        .WithAuthor(socketUser.ToString(),
                            socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                        .WithTitle("Unauthorized User")
                        .WithDescription($"You are not allowed to use this command. Only Super Admin can Access")
                        .WithColor(Color.Red)
                        .WithCurrentTimestamp()
                        .Build()
                    );

                break;
            default:
                // If the permission level is not set, it is bound to end up here
                await command.RespondAsync(embed: new EmbedBuilder()
                    .WithAuthor(socketUser.ToString(),
                        socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl())
                    .WithTitle("Undefined Permission Level")
                    .WithDescription($"The command accessed does not imply which permission level allows for access")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
                );
                break;
        }
    }
}
