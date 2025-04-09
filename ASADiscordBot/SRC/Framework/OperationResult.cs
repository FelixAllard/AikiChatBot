namespace ASADiscordBot.Framework;

public class OperationResult<T>
{
    public T Result { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    
    
}