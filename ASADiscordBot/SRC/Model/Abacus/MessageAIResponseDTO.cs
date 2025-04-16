namespace ASADiscordBot.Model.Abacus;

/// <summary>
/// Root is what we use!
/// </summary>
public class MessageAIResponseDTO
{
    public bool Success { get; set; }
    public Result Result { get; set; }
}

public class Result
{
    public List<Message> Messages { get; set; }
    public List<SearchResultWrapper> Search_Results { get; set; }
    public Dictionary<string, object> Filter_Key_Values { get; set; }
    public object Score_Boost_For_Filters { get; set; }
    public string Deployment_Conversation_Id { get; set; }
}

public class Message
{
    public bool Is_User { get; set; }
    public string Text { get; set; }
}

public class SearchResultWrapper
{
    public int Msg_Id { get; set; }
    public List<SearchResult> Results { get; set; }
}

public class SearchResult
{
    public string Answer { get; set; }
    public Context Context { get; set; }
    public List<string> Image_Ids { get; set; }
    public double Score { get; set; }
    public Metadata Metadata { get; set; }
}

public class Context
{
    public string _Chunk_Id { get; set; }
    public string Chunked_Document { get; set; }
    public List<int> Offset { get; set; }
    public List<int> _Page_Numbers { get; set; }
    public List<object> _Bounding_Boxes { get; set; }
    public List<object> Markdown_Features { get; set; }
}

public class Metadata
{
    public string File_Path { get; set; }
}
