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
    public int AvailableWorkers { get; set; }
    
    public int MaxWorkers { get; }
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
        int workersNumber, 
        HttpClient httpClient, 
        IConfiguration config, 
        SherwebDbContext sherwebDbContext,
        Dictionary<string, string> keys
    )
    {
        MaxWorkers = workersNumber;
        AvailableWorkers = MaxWorkers;
        HttpClient = httpClient;
        Configuration = config;
        SherwebDbContext = sherwebDbContext;
        Keys = keys;
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
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Result = (
                        true, 
                        getAllCustomerRequest,
                        false
                    )
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