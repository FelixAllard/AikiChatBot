using System.Text.Json.Serialization;

namespace ASADiscordBot.Model.Abacus;

public class MessageAIRequestDTO
{
    public string DeploymentToken { get; set; }
    public string DeploymentId { get; set; }
    public string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string DeploymentConversationId { get; set; }
}
