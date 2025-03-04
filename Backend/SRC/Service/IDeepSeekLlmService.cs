namespace Backend.Service;

public interface IDeepSeekLlmService
{
    /// <summary>
    /// Generates text based on the provided prompt using the DeepSeek LLM model
    /// </summary>
    /// <param name="prompt">The input prompt for text generation</param>
    /// <param name="maxLength">Maximum length of the generated text</param>
    /// <returns>The generated text response</returns>
    Task<string> GenerateTextAsync(string prompt, int maxLength = 200);
}