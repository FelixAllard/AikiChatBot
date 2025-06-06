﻿using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ASADiscordBot.SlashCommand.Command.Info;

public class HelloWorldSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "hello-world";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Open;

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider)
    {
        builder = new SlashCommandBuilder();
        builder.WithName(Name);
        
        builder.WithDescription("Simple test command to make sure the application is working");
        builder.AddOption(new SlashCommandOptionBuilder()
            .WithName("rating")
            .WithDescription("The rating your willing to give our bot")
            .WithRequired(true)
            .AddChoice("Terrible", 1)
            .AddChoice("Meh", 2)
            .AddChoice("Good", 3)
            .AddChoice("Lovely", 4)
            .AddChoice("Excellent!", 5)
            .WithType(ApplicationCommandOptionType.Integer)
        );
        
        HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        
        return new OperationResult<bool>()
        {
            IsSuccess = true,
            Message = "Built",
            Result = true
        };
    }

    public async Task HandleClientCall(SocketSlashCommand command, SocketUser caller)
    {
        var guildUser = command.User ;
        List<string> differentHello = new List<string>()
        {
            $"Hey {guildUser.Username}!",
            $"Hi {guildUser.Username}, how can I help you?",
            $"Hello World! {guildUser.Username}!"
        };
        var roleList = "";
        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(differentHello[new Random().Next(0, differentHello.Count)])
            .WithColor(Color.Green)
            .WithCurrentTimestamp();
        
        var builder = new ComponentBuilder()
            .WithButton("label", "custom-id"); 
        
        await command.RespondAsync(embed: embedBuiler.Build(), components: builder.Build());
    }
}