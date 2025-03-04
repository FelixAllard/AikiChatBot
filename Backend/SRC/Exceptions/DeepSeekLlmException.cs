namespace Backend.Exceptions;

public class DeepSeekLlmException : Exception
{
    public DeepSeekLlmException(string message) : base(message) { }
    public DeepSeekLlmException(string message, Exception innerException) : base(message, innerException) { }
}