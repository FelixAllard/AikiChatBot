using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;
using Azure;
using Microsoft.IdentityModel.Tokens;

namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebWorkers : IHttpWorker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SherwebWorkers> _logger;
    private Request CurrentRequest { get; set; }
    private Dictionary<string,string> _credentials;
    private Request _currentRequest;
    private float _currentTimeout;
    private Request _authentificationRequest;
    private int authorizationTryCount = 0;
    public SherwebWorkers(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }


    public async Task<OperationResult<IHttpWorker>> PrepareWorker(Dictionary<string, string> credentials)
    {
        _credentials = credentials;
        return new OperationResult<IHttpWorker>()
        {
            Status = OperationResultStatus.Success,
            Result = this,
            Exception = null,
            Message = "Preparations are complete! Worker is now operational",
        };
    }

    public async Task<OperationResult<JsonContent>> SendRequest(Request request, float timeout = 3000)
    {
        _currentRequest = request;
        _currentTimeout = timeout;
        JsonContent finalResult = null;
        try
        {
            var response = _currentRequest.SendRequest(_currentTimeout);
            if (response.Result.Status == OperationResultStatus.Failed)
                return await Task.FromResult(new OperationResult<JsonContent>()
                {
                    Status = OperationResultStatus.Failed,
                    Message = "Unable to finish operation because critical error happened in the the request",
                    Exception = response.Exception,
                    Result = response.Result.Result
                    
                });
            if (response.Result.Status == OperationResultStatus.PartialSuccess)
            {
                if (authorizationTryCount > 0)
                    return await Task.FromResult(new OperationResult<JsonContent>()
                    {
                        Status = OperationResultStatus.Critical,
                        Exception = new Exception("Unable to authenticate after trying"),
                        Result = response.Result.Result,
                        Message = "Authentication failed",
                    });
                // We want to avoid infinite recursive calls
                authorizationTryCount++;
                var result = await HandleUnAuthorized(_authentificationRequest, 5000);
                if(result.Status == OperationResultStatus.Success)
                    // Recursive call so that it is easier to understand
                    return await SendRequest(request, timeout);
            }
            finalResult = response.Result.Result;
        }
        catch (Exception e)
        {
            return new OperationResult<JsonContent>()
            {
                Status = OperationResultStatus.Failed,
                Exception = e,
                Message = e.Message,
                Result = null
            };
        }

        return await Task.FromResult(new OperationResult<JsonContent>()
        {
            Status = OperationResultStatus.Success,
            Result = finalResult,
            Message = "Successfully sent request to Sherweb",
        });

    }
    

    public async Task<OperationResult<string>> HandleUnAuthorized(Request request, float timeout)
    {
        string bearerToken = "";
        try
        {
            var response = _authentificationRequest.SendRequest();
            if (response.Result.Status == OperationResultStatus.Failed)
                return new OperationResult<string>()
                {
                    Status = OperationResultStatus.Failed,
                    Message = "Unable to finish operation because critical error happened in the the request",
                    Exception = response.Exception,
                    Result = response.Result.Result.ToString()
                    
                };
            var jsonObject = await response.Result.Result.ReadFromJsonAsync<Dictionary<string, object>>();
            //Retrieving the token from the Authorization string
            bearerToken = jsonObject?["access_token"]?.ToString();
            if(bearerToken.IsNullOrEmpty())
                throw new InvalidCastException("Unable to finish operation because critical " +
                                               "error happened in the the request. " +
                                               "The json gotten back from the api was invalid");
        }
        catch (Exception e)
        {
            return await Task.FromResult(new OperationResult<string>()
            {
                Status = OperationResultStatus.Failed,
                Message = "Unable to finish operation because critical error happened in the the request",
                Exception = e,
                Result = bearerToken
                    
            });
        }
        return await Task.FromResult(new OperationResult<string>()
        {
            Status = OperationResultStatus.Success,
            Result = bearerToken,
            Message = "Successfully Generated New Bearer Token",
        });
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