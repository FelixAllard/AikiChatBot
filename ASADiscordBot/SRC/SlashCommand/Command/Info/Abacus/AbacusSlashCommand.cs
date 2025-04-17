using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ASADiscordBot.Database;
using ASADiscordBot.Database.Model;
using ASADiscordBot.Framework;
using ASADiscordBot.Model;
using ASADiscordBot.Model.Abacus;
using ASADiscordBot.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ASADiscordBot.SlashCommand.Command.Info.Abacus;

public class AbacusSlashCommand : ISlashCommand
{
    public bool IsGlobal { get; set; }
    public SlashCommandBuilder builder { get; set; }
    public string Name { get; } = "ask";
    public IServiceProvider ServiceProvider { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }

    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Listed;

    public async Task<OperationResult<bool>> Init(IServiceProvider serviceProvider = null)
    {
        ServiceProvider = serviceProvider;
        
        builder = new SlashCommandBuilder();
        builder.WithName(Name)
                .WithDescription("Ask questions to the AIKI Chatbot and get answers easily")
                /*.AddOption(new SlashCommandOptionBuilder()
                    .WithName("create")
                    .WithDescription("Create a new conversation")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(
                        "question", 
                        ApplicationCommandOptionType.String, 
                        "The question for the Chatbot", 
                        isRequired: true
                    )
                )*/
                .AddOption(
                    "question", 
                    ApplicationCommandOptionType.String, 
                    "The question for the Chatbot", 
                    isRequired: true
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
    await command.DeferAsync(); // Defer immediately to avoid timeout

    // Get the user's question
    var valueQuestion = command.Data.Options.First().Value?.ToString();

    // Build the HttpClient
    var client = HttpClientFactory.CreateClient();
    client = HttpClientFormatter.BuildAbacusHttpClient(client).Result; // Assume you make this method async

    Identity user;
    bool createNewChat = false;
    string lastChatID = "";

    using (var scope = ServiceProvider.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();

        // Get the user and their last chat info
        user = context.Identities.FirstOrDefault(x => x.DiscordUserId == command.User.Id);
        lastChatID = user.LastChat;
        DateTime lastChat = user.LastQuerry;
        createNewChat = lastChat.AddHours(1) <= DateTime.UtcNow;

        if (string.IsNullOrEmpty(user.LastChat))
            createNewChat = false;

        user.LastQuerry = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    // Prepare request DTO
    var requestDto = new MessageAIRequestDTO
    {
        DeploymentId = Environment.GetEnvironmentVariable("DEPLOYMENT_ID"),
        DeploymentToken = Environment.GetEnvironmentVariable("ABACUS_DEPLOYMENT_TOKEN"),
        Message = valueQuestion,
        DeploymentConversationId = createNewChat ? null : lastChatID
    };

    var json = JsonSerializer.Serialize(requestDto, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });

    var content = new StringContent(json, Encoding.UTF8, "application/json");

    Console.WriteLine("Sending Request");
    var response = await client.PostAsync("https://apps.abacus.ai/api/v0/getConversationResponse", content);
    Console.WriteLine("Received Answer");

    if (response.IsSuccessStatusCode)
    {
        var jsonString = await response.Content.ReadAsStringAsync();
        var messageResponse = JsonSerializer.Deserialize<MessageAIResponseDTO>(jsonString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (messageResponse?.Success == true)
        {
            var result = messageResponse.Result;

            // Update user's chat ID
            using (var scope = ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ASADbContext>();
                user = context.Identities.FirstOrDefault(x => x.DiscordUserId == command.User.Id);
                Console.WriteLine(result.Deployment_Conversation_Id);
                user.LastChat = result.Deployment_Conversation_Id;
                await context.SaveChangesAsync();
            }

            // Get messages
            var messageSent = result.Messages[result.Messages.Count - 2];
            var messageReceived = result.Messages[result.Messages.Count - 1];

            // Split the message into 2000-character chunks
            const int maxLength = 2000;
            var fullText = messageReceived.Text;
            var chunks = new List<string>();

            for (int i = 0; i < fullText.Length; i += maxLength)
            {
                chunks.Add(fullText.Substring(i, Math.Min(maxLength, fullText.Length - i)));
            }

            // Send all but the last chunk as simple messages
            for (int i = 0; i < chunks.Count - 1; i++)
            {
                await command.FollowupAsync(text: chunks[i]);
            }

            // Send the last chunk with the embed
            var embed = new EmbedBuilder()
                .WithAuthor(caller.ToString(), caller.GetAvatarUrl() ?? caller.GetDefaultAvatarUrl())
                .WithTitle("Success in the query")
                .WithDescription($"Answer for the question: {messageSent.Text}")
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();

            await command.FollowupAsync(text: chunks.Last(), embed: embed);
        }
        else
        {
            await command.FollowupAsync("There was a problem with the AI response.");
        }
    }
    else
    {
        Console.WriteLine($"Request failed: {response.StatusCode}");
        await command.FollowupAsync("The request to the AI failed. Please try again later.");
    }
}


}
