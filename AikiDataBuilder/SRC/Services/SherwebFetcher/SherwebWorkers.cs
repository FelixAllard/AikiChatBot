using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebWorkers : IHttpWorker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SherwebWorkers> _logger;
    private IRequest CurrentRequest { get; set; }
    public SherwebWorkers(
        IConfiguration configuration
        )
    {
        _configuration = configuration;
        
    }
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