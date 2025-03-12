using AikiDataBuilder.Model.SystemResponse;

namespace AikiDataBuilder.Services.Workers;

public interface IHttpRequest
{
    public Task<OperationResult<JsonContent>> SendRequest(IRequest request, float timeout = 3000);
    public Task<OperationResult<JsonContent>> GetInformationFromApi(IRequest request);
    public Task<OperationResult<JsonContent>> ResetAuthorization(IAuthorizationRequest request);
}