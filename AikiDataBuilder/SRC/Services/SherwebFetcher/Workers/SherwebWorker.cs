using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.SherwebFetcher.Workers;

public class SherwebWorker : IHttpWorker
{
    

    public async Task<OperationResult<IHttpWorker>> PrepareWorker()
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> SendRequest(IRequest request, float timeout = 3000)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> HandleTimeout(IRequest request, float timeout)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> HandleUnAuthorized(IRequest request, float timeout)
    {
        throw new NotImplementedException();
    }

    public OperationResult<string> CleanUpWorker()
    {
        throw new NotImplementedException();
    }
}