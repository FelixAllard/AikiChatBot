using AikiDataBuilder.Model.SystemResponse;

namespace AikiDataBuilder.Services.Workers;
/// <summary>
/// Http workers will be responsible to send 
/// </summary>
public interface IHttpWorker
{
    /// <summary>
    /// Creates the workers using all the information for the specific API
    /// </summary>
    /// <returns>Operation Result Holding the reference to itself</returns>
    /// <param name="credentials">Will hold all the credential. Reference to a dictionary because it will also hold the bearer</param>
    public Task<OperationResult<IHttpWorker>> PrepareWorker(Dictionary<string,string> credentials);
    

    /// <summary>
    /// Will send the Request to the specified IRequest
    /// </summary>
    /// <param name="request">The Request to do</param>
    /// <param name="timeout">The amount of time to wait before considering the Request Timed Out</param>
    /// <returns>Returns and Operation Result with the content received from the external API</returns>
    public Task<OperationResult<JsonContent>> SendRequest(Request request, float timeout = 3000);
    /// <summary>
    /// Responsible to handle the request in case the request Times out
    /// </summary>
    /// <param name="request">The same IRquest that failed before</param>
    /// <param name="timeout">The ammount of time to wait before considering the request timed out</param>
    /// <returns>Returns the response, or and Error</returns>
    public Task<OperationResult<JsonContent>> HandleTimeout(Request request, float timeout);
    /// <summary>
    /// In case of an UnAuthorized, this function will be called which will handle making sure to refresh the key.
    /// <b> This should not happen but we are prepared for it!</b>
    /// </summary>
    /// <remarks>This will do a recursive call to <see cref="SendRequest(Request, float)"/> if it is able to authenticate</remarks>
    /// <param name="request">The request that failed because of an Unauthorized</param>
    /// <param name="timeout">The timeout allowed, this will be ignored for the authorization request as it is nescessary</param>
    /// <returns>Return a JSON Content, since it is a recursive call, it will return what <see cref="SendRequest(Request, float)"/> would return</returns>
    public Task<OperationResult<JsonContent>> HandleUnAuthorized(Request request, float timeout);
    /// <summary>
    /// This function is responsible to make sure the workers has cleaned up everything before moving on to the next
    /// </summary>
    /// <returns></returns>
    public OperationResult<string> CleanUpWorker();


}