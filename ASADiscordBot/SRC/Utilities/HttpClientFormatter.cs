using System.Net.Http.Headers;
using ASADiscordBot.Framework;

namespace ASADiscordBot.Utilities;

public static class HttpClientFormatter
{
    /// <summary>
    /// Format the HTTP client for to connect to the AIKI Data builder
    /// </summary>
    /// <param name="httpClient">The http client to format</param>
    /// <returns>An operation Result with the HTTP client</returns>
    public static OperationResult<HttpClient> BuildAikiDataBuilderHttpClient(HttpClient httpClient)
    {
        var client = httpClient;
        
        if(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_DOCKER") == "true")
            client.BaseAddress = new Uri("http://aikidatabuilder:5173");
        else
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
    /// <summary>
    /// Will format a HTTP client to connect to abacus rest api
    /// </summary>
    /// <param name="httpClient">The http client to format</param>
    /// <returns>The formatted HTTP client in a OperationResult</returns>

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