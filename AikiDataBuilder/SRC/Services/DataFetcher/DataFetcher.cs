using System.Diagnostics;
using System.Reflection;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher;

namespace AikiDataBuilder.Services.DataFetcher;

/// <summary>
/// Responsible to fetch from all the APIs asynchronously
/// </summary>
public class DataFetcher
{
    public List<IApiFetcher> Fetchers { get; set; }
    private readonly HttpClient HttpClient;
    private readonly IConfiguration Configuration;
    private readonly SherwebDbContext SherwebDbContext;
    private readonly ILogger<DataFetcher> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DataFetcher(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        SherwebDbContext sherwebDbContext,
        ILogger<DataFetcher> logger,
        IServiceProvider serviceProvider
        
    )
    {
        HttpClient = httpClientFactory.CreateClient();
        Configuration = configuration;
        SherwebDbContext = sherwebDbContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogInformation("WE REACHED THIS STATEMENT");
    }
    
    /// <summary>
    /// This will get all API Fetchers and store them into the Fetchers variable
    /// </summary>
    /// See the following file to understand better
    /// <see cref="./IApiFetcher"/>
    /// <returns>Returns the result of the operation or the Error</returns>
    public OperationResult<List<IApiFetcher>> GetApiFetchers()
    {
        OperationResult<List<IApiFetcher>> result;
        var interfaceType = typeof(IApiFetcher);
        try
        {
            // Use dependency injection to resolve instances
            List<IApiFetcher> apiFetchers = _serviceProvider.GetServices<IApiFetcher>().ToList();

            Fetchers = apiFetchers;
            result = new OperationResult<List<IApiFetcher>>(
                "Successfully fetched data", 
                OperationResultStatus.Success, 
                null, 
                apiFetchers
            );
        }
        catch (Exception ex)
        {
            result = new OperationResult<List<IApiFetcher>>(
                $"Failed to fetch data: {ex.Message}", 
                OperationResultStatus.Failed, 
                ex
            );
        }
        return result;
    }

    /// <summary>
    /// Runs all Api Fetchers' FetchDataAsync method asynchronously and measures execution time.
    /// Gives the current DateTime so that it knows how many calls it can make
    /// </summary>
    public async Task ExecuteAllFetchersAsync()
    {
        if (Fetchers == null || Fetchers.Count == 0)
        {
            _logger.LogWarning("No API Fetchers configured");
            return;
        }
        // This will iterate Through every API callers and start the Fetching process
        var tasks = Fetchers.Select(fetcher => Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            fetcher.CreateWorkers();// Create the workers
            await fetcher.GetInformationFromApi(DateTime.Now); // Call the async method on each fetcher
            stopwatch.Stop();

            Console.WriteLine($"{fetcher.GetType().Name} execution time: {stopwatch.ElapsedMilliseconds} ms");
        })).ToList();

        await Task.WhenAll(tasks);
    }
}

