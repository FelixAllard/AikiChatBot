using System.Collections.Concurrent;
using System.ComponentModel.Design;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher.Requests;
using AikiDataBuilder.Services.Workers;


namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebRequestManager : IRequestManager
{
    public ConcurrentQueue<Request> Requests { get; set; }
    public int AvailableWorkers { get; set; } = 3;

    public int MaxWorkers { get; } = 3;
    private IConfiguration Configuration { get; }
    private HttpClient HttpClient { get; }
    private SherwebDbContext SherwebDbContext { get; }
    private Dictionary<string, string> Keys { get; }
    
    
    private string bearerToken;
    public bool AllWorkersAvailable
    {
        get
        {
            if (MaxWorkers==AvailableWorkers)
            {
                return true;
            }
            return false;
        }
    }

    private int[] retrievalStep = [0, 0];
    private bool awaitingOtherWorkers = false;


    public SherwebRequestManager(
        HttpClient httpClient, 
        IConfiguration config, 
        SherwebDbContext sherwebDbContext
    )
    {
        HttpClient = httpClient;
        Configuration = config;
        SherwebDbContext = sherwebDbContext;
        Keys = GetCredentials().Result;
    }
    public OperationResult<Dictionary<string, string>> GetCredentials()
    {
        
        string baseUrl = Configuration["SherwebCredentials:BaseUrl"];
        string subscriptionKey = Configuration["SherwebCredentials:SubscriptionKey"];
        string clientId = Configuration["SherwebCredentials:ClientId"];
        string clientSecret = Configuration["SherwebCredentials:ClientSecret"];
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
    

    public OperationResult<bool> ReturnWorker(int numberOfWorkers = 1)
    {
        AvailableWorkers -= numberOfWorkers;
        return new OperationResult<bool>()
        {
            Message = $"Worker count {numberOfWorkers}",
            Result = AllWorkersAvailable,
            Exception = null,
            Status = OperationResultStatus.Success
        };
    }
    

    
    public async Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> GetNextRequest()
    {
        if(AllWorkersAvailable)
            awaitingOtherWorkers = false;
        if (awaitingOtherWorkers)
            return new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
            {
                Message = $"Currently awaiting all workers to be available",
                Status = OperationResultStatus.PartialSuccess,
                Result = (
                    false, 
                    null, 
                    false
                )
            };
        // Here we will put all the steps inside a switch
        switch (retrievalStep[0])
        {
            case 0:
                var getAllCustomerRequest = new GetAllCustomers(
                    HttpClient,
                    SherwebDbContext
                );
                getAllCustomerRequest.SetBearerToken(bearerToken);
                getAllCustomerRequest.SetHeaderVariables(new Dictionary<string, string>()
                {
                    {
                        "Ocp-Apim-Subscription-Key", 
                        Keys[
                            "SubscriptionKey"
                        ]
                    }
                });
                retrievalStep[0] = 1;
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Result = (
                        true, 
                        getAllCustomerRequest,
                        false
                    ),
                    Message = "Get all customers successfully sent",
                    Status = OperationResultStatus.Success
                });
            default:
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Message = "Not Finding a request to give back",
                    Result =(
                    true,
                    null,
                    true
                    ),
                    Status = OperationResultStatus.Success
                    
                });
        }

        return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
        {
            Message = $"Finished all steps of retrieval from sherweb. Stopping retrieval",
            Status = OperationResultStatus.Success,
            Result = (
                false, 
                null!,
                true
            )
        });
    }
    
}