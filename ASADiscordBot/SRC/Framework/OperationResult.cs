namespace ASADiscordBot.Framework;

/// <summary>
/// Can be used to return stuff with additional information
/// </summary>
/// <typeparam name="T">What is the type of the return</typeparam>
public class OperationResult<T>
{
    public T Result { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    
    
}