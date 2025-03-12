using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.SherwebFetcher;

/// <summary>
/// The API fetcher for Sherweb
/// </summary>
public class SherwebFetcher : IApiFetcher
{
    public List<IHttpRequest> Workers { get; set; }
    public ILogger<IApiFetcher> Logger { get; set; }
    private readonly IConfiguration _configuration;


    private OperationResult<Dictionary<string, string>> GetCredentials()
    {
        string baseUrl = _configuration["SherwebCredentials:BaseUrl"];
        string subscriptionKey = _configuration["SherwebCredentials:SubscriptionKey"];
        string clientId = _configuration["SherwebCredentials:ClientId"];
        string clientSecret = _configuration["SherwebCredentials:ClientSecret"];
        if (
            string.IsNullOrWhiteSpace(baseUrl)
            || string.IsNullOrWhiteSpace(subscriptionKey)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret)
        )
        {
            return new OperationResult<Dictionary<string, string>>
            {
                Result = new Dictionary<string, string>
                {
                    { "BaseUrl", baseUrl },
                    { "SubscriptionKey", subscriptionKey },
                    { "ClientId", clientId },
                    { "ClientSecret", clientId },
                },
                Error = new FormatException("The credentials fetch failed for one or more values"),
                Message = "Make sure the credentials are filled in the appsettings.json file.",
                Status = OperationResultStatus.Critical
            };
        }

        return new OperationResult<Dictionary<string, string>>
        {
            Result = new Dictionary<string, string>
            {
                { "BaseUrl", baseUrl },
                { "SubscriptionKey", subscriptionKey },
                { "ClientId", clientId },
                { "ClientSecret", clientId },
            },
            Message = "All the information was available! Successfully fetched from AppSettings.json file.",
            Status = OperationResultStatus.Success
        };
    }
    public async Task<OperationResult<List<IHttpRequest>>> CreateWorkers(int workersCount)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<List<IHttpRequest>>> CreateWorkers()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<List<IHttpRequest>>> CreateWorkers(List<IHttpRequest> workers)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> GetInformationFromApi(DateTime currentDateTime)
    {
        throw new NotImplementedException();
    }
}