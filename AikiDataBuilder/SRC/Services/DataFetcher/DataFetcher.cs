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
        
        // Null check for IHttpClientFactory
        if (httpClientFactory == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory), "HttpClientFactory cannot be null");
        }

        // Null check for IConfiguration
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
        }

        // Null check for SherwebDbContext
        if (sherwebDbContext == null)
        {
            throw new ArgumentNullException(nameof(sherwebDbContext), "SherwebDbContext cannot be null");
        }

        // Null check for ILogger<DataFetcher>
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        }

        // Null check for IServiceProvider
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider), "ServiceProvider cannot be null");
        }

        // Initialize class variables
        HttpClient = httpClientFactory.CreateClient();
        Configuration = configuration;
        SherwebDbContext = sherwebDbContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
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
            var temp = _serviceProvider.GetServices<IApiFetcher>();
            _logger.LogWarning("TEMPREACHED");
            List<IApiFetcher> apiFetchers = temp.ToList();
            if (apiFetchers == null)
            {
                _logger.LogError("API Fetchers collection is null.");
                result = new OperationResult<List<IApiFetcher>>(
                    "Failed to fetch data: API Fetchers list is null",
                    OperationResultStatus.Failed
                );
            }
            else
            {
                _logger.LogInformation("Fetched them all!");
                Fetchers = apiFetchers;
                result = new OperationResult<List<IApiFetcher>>(
                    "Successfully fetched all API Fetchers",
                    OperationResultStatus.Success,
                    null,
                    apiFetchers
                );
            }
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

