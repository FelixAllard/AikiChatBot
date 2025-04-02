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
    public SherwebWorkers(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
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
            await _currentRequest.AddToDatabase(finalResult);
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

    

    /*public async Task<OperationResult<string>> HandleUnAuthorized(Request request, float timeout)
    {
        string bearerToken = "";
        try
        {
            var response = _authentificationRequest.SendRequest();
            if (response.Result.Status == OperationResultStatus.Failed)
                return await Task.FromResult(new OperationResult<string>()
                {
                    Status = OperationResultStatus.Failed,
                    Message = "Unable to finish operation because critical error happened in the the request",
                    Exception = response.Exception,
                    Result = response.Result.Result.ToString()
                    
                });
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
    }*/
    

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