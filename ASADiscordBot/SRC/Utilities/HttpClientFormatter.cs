using System.Net.Http.Headers;
using ASADiscordBot.Framework;

namespace ASADiscordBot.Utilities;

public static class HttpClientFormatter
{
    public static OperationResult<HttpClient> BuildAikiDataBuilderHttpClient(HttpClient httpClient)
    {
        var client = httpClient;
        client.BaseAddress = new Uri("http://localhost:5173");
        // May need to be changed in case where we have a larger Sherweb database
        client.Timeout = TimeSpan.FromSeconds(2000);
        
        return new OperationResult<HttpClient>()
        {
            Message = "Built Client",
            IsSuccess = true,
            Result = client
        };
    }

    public static OperationResult<HttpClient> BuildAbacusHttpClient(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri("https://api.abacus.ai/api/v0");
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("apiKey", Environment.GetEnvironmentVariable("ABACUS_API_KEY"));
        return new OperationResult<HttpClient>()
        {
            Message = "Built Client",
            IsSuccess = true,
            Result = httpClient
        };
    }
    
}