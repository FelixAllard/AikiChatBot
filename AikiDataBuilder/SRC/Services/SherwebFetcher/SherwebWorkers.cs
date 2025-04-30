using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;
using Azure;
using Microsoft.IdentityModel.Tokens;

namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebWorkers : IHttpWorker
{
    private readonly IConfiguration _configuration;
    private Request CurrentRequest { get; set; }
    private Dictionary<string,string> _credentials;
    private Request _currentRequest;
    private float _currentTimeout;
    //private Request _authentificationRequest;
    private int authorizationTryCount = 0;
    private IServiceScopeFactory _scope;
    public SherwebWorkers(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory
    )
    {
        _configuration = configuration;
        _scope = scopeFactory;
    }


    public int WorkerId { get; set; }

    public async Task<OperationResult<IHttpWorker>> PrepareWorker(Dictionary<string, string> credentials)
    {
        _credentials = credentials;
        return await Task.FromResult(new OperationResult<IHttpWorker>()
        {
            Status = OperationResultStatus.Success,
            Result = this,
            Exception = null,
            Message = "Preparations are complete! Worker is now operational",
        });
    }
    public async Task<OperationResult<string>> SendRequest(Request request, float timeout = 3000)
    {
        _currentRequest = request;
        _currentTimeout = timeout;
        string finalResult = null;

        try
        {
            var response = await _currentRequest.SendRequest(_currentTimeout); // Ensure async execution

            if (response.Status == OperationResultStatus.Failed)
            {
                return new OperationResult<string>
                {
                    Status = OperationResultStatus.Failed,
                    Message = "Unable to finish operation because a critical error happened in the request.",
                    Exception = response.Exception,
                    Result = response.Result
                };
            }

            // Handle PartialSuccess (likely Unauthorized response)
            if (response.Status == OperationResultStatus.PartialSuccess)
            {
                return new OperationResult<string>
                {
                    Status = OperationResultStatus.PartialSuccess,
                    Message = response.Message,
                    Result = response.Result,
                    Exception = response.Exception
                };
            }

            finalResult = response.Result;
            Console.WriteLine(finalResult);

            // Handle database update

            using (var scope = _scope.CreateScope())
            {
                await _currentRequest.AddToDatabase(scope,finalResult); 
            }
            
        }
        catch (Exception e)
        {
            return new OperationResult<string>
            {
                Status = OperationResultStatus.Failed,
                Exception = e,
                Message = e.Message,
                Result = null
            };
        }

        return new OperationResult<string>
        {
            Status = OperationResultStatus.Success,
            Result = finalResult,
            Message = "Successfully sent request to Sherweb."
        };
    }

    public OperationResult<string> CleanUpWorker()
    {
        authorizationTryCount = 0;
        return new OperationResult<string>()
        {
            Result = string.Empty,
            Status = OperationResultStatus.Success
        };
    }
}