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
    
}