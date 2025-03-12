using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.SherwebFetcher;

/// <summary>
/// The API fetcher for Sherweb
/// </summary>
public class SherwebFetcher : IApiFetcher
{
    public List<IHttpWorker> Workers { get; set; }
    public ILogger<IApiFetcher> Logger { get; set; }
    private readonly IConfiguration _configuration;
    private readonly int _numberOfWorkers = 3;
    protected Dictionary<string, string> keys;


    public OperationResult<Dictionary<string, string>> GetCredentials()
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
                Exception = new FormatException("The credentials fetch failed for one or more values"),
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
    public OperationResult<List<IHttpWorker>> CreateWorkers(int workersCount)
    {
        throw new NotImplementedException();
    }

    public OperationResult<List<IHttpWorker>> CreateWorkers()
    {
        var credidentials = GetCredentials();
        if (credidentials.Status != OperationResultStatus.Success)
        {
            Logger.LogError($"The program encountered {credidentials.Status.ToString()} " +
                            $"while retrieving the information : {credidentials.Message}"
            );
            // Resturn a failure OperationResult in order to go back up in the call stack
            return new OperationResult<List<IHttpWorker>>()
            {
                Exception = credidentials.Exception,
                Status = OperationResultStatus.Failed,
                Message = credidentials.Message
            };

        }
        for (int i = 0; i < _numberOfWorkers; i++)
        {
            var sherwebWorker = new SherwebWorkers(_configuration);
            sherwebWorker.PrepareWorker(credidentials.Result);
            Workers.Add(
                new SherwebWorkers(
                    _configuration
                    )
                );
            
        }

        return new OperationResult<List<IHttpWorker>>()
        {
            Status = OperationResultStatus.Success,
            Result = Workers,
            Message = $"Successfully created the {Workers.Count} workers."
        };
    }

    public OperationResult<List<IHttpWorker>> CreateWorkers(List<IHttpWorker> workers)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> GetInformationFromApi(DateTime currentDateTime)
    {
        throw new NotImplementedException();
    }
}