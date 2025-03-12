using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebWorkers : IHttpWorker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SherwebWorkers> _logger;
    private Request CurrentRequest { get; set; }
    private readonly Dictionary<string,string> _credentials;
    public SherwebWorkers(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }


    public async Task<OperationResult<IHttpWorker>> PrepareWorker(Dictionary<string, string> credentials)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> SendRequest(Request request, float timeout = 3000)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> HandleTimeout(Request request, float timeout)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<JsonContent>> HandleUnAuthorized(Request request, float timeout)
    {
        throw new NotImplementedException();
    }

    public OperationResult<string> CleanUpWorker()
    {
        throw new NotImplementedException();
    }
}