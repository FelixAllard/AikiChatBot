using System.Text.Json.Serialization;

namespace ASADiscordBot.Model.Abacus;

/// <summary>
/// Will be used when asking querries to abacus ai
/// </summary>
public class MessageAIRequestDTO
{
    public string DeploymentToken { get; set; }
    public string DeploymentId { get; set; }
    /// <summary>
    /// The message we are asking the AI
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// This is the conversation that is currently ongoing
    /// </summary>

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string DeploymentConversationId { get; set; }
}
