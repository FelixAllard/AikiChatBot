using Backend.Exceptions;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Service
{
    public class DeepSeekLlmService : IDeepSeekLlmService
    {
        private readonly string _endpoint = "http://localhost:8000";
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeepSeekLlmService> _logger;

        public DeepSeekLlmService(
            HttpClient httpClient,
            ILogger<DeepSeekLlmService> logger
            )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure the base address for the HttpClient
            _httpClient.BaseAddress = new Uri(_endpoint);
        }

        public async Task<string> GenerateTextAsync(string prompt, int maxLength = 200)
        {
            try
            {
                _logger.LogInformation("Sending request to DeepSeek LLM API with prompt: {Prompt}", prompt);

                var requestBody = new
                {
                    prompt = prompt,
                    max_length = maxLength
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("generate", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GenerateResponse>(
                    responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Received response from DeepSeek LLM API");
                return result?.Response ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling DeepSeek LLM API");
                throw new DeepSeekLlmException("Failed to generate text from DeepSeek LLM API", ex);
            }
        }

        private class GenerateResponse
        {
            public string Response { get; set; }
        }
    }

}