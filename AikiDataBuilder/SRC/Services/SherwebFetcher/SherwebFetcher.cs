using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.DataFetcher;
using AikiDataBuilder.Services.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AikiDataBuilder.Services.SherwebFetcher;

/// <summary>
/// The API fetcher for Sherweb
/// </summary>
public class SherwebFetcher : IApiFetcher
{
    public List<IHttpWorker> Workers { get; set; }
    public ILogger<IApiFetcher> Logger { get; set; }
    
    private readonly int _numberOfWorkers = 3;
    protected Dictionary<string, string> keys;
    private IRequestManager requestManager;
    private readonly SemaphoreSlim _semaphore;
    
    
    private IConfiguration _configuration;
    private HttpClient _httpClient;
    private SherwebDbContext _sherwebDbContext;


    public OperationResult<bool> Init()
    {
        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _httpClient = new HttpClient();
            var tempDbContext = new DbContextOptionsBuilder<SherwebDbContext>()
                .UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
                .Options;
            _sherwebDbContext = new SherwebDbContext(tempDbContext);
            keys = GetCredentials().Result;
            
            requestManager = new SherwebRequestManager(
                _numberOfWorkers, 
                _httpClient,
                _configuration, 
                _sherwebDbContext, 
                keys
            );
        }
        catch (Exception e)
        {
            return new OperationResult<bool>()
            {
                Status = OperationResultStatus.Critical,
                Exception = e,
                Message = e.Message,
                Result = false
            };
        }
        return new OperationResult<bool>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully created Dependencies",
            Result = false
        };
    }

    public OperationResult<Dictionary<string, string>> GetCredentials()
    {
        string baseUrl = _configuration["SherwebCredentials:BaseUrl"];
        string subscriptionKey = _configuration["SherwebCredentials:SubscriptionKey"];
        string clientId = _configuration["SherwebCredentials:ClientId"];
        string clientSecret = _configuration["SherwebCredentials:ClientSecret"];
        if (
            string.IsNullOrWhiteSpace(baseUrl)
            || string.IsNullOrWhiteSpace(subscriptionKey)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret)
        )
        {
            return new OperationResult<Dictionary<string, string>>
            {
                Result = new Dictionary<string, string>
                {
                    { "BaseUrl", baseUrl },
                    { "SubscriptionKey", subscriptionKey },
                    { "ClientId", clientId },
                    { "ClientSecret", clientId },
                },
                Exception = new FormatException("The credentials fetch failed for one or more values"),
                Message = "Make sure the credentials are filled in the appsettings.json file.",
                Status = OperationResultStatus.Critical
            };
        }

        return new OperationResult<Dictionary<string, string>>
        {
            Result = new Dictionary<string, string>
            {
                { "BaseUrl", baseUrl },
                { "SubscriptionKey", subscriptionKey },
                { "ClientId", clientId },
                { "ClientSecret", clientId },
            },
            Message = "All the information was available! Successfully fetched from AppSettings.json file.",
            Status = OperationResultStatus.Success
        };
    }
    [Obsolete("Doesn't take into consideration the API's rate limit of concurrent request")]
    public OperationResult<List<IHttpWorker>> CreateWorkers(int workersCount)
    {
        var credidentials = GetCredentials();
        
        if (credidentials.Status != OperationResultStatus.Success)
        {
            
            Logger.LogError($"The program encountered {credidentials.Status.ToString()} " +
                            $"while retrieving the information : {credidentials.Message}"
            );
            
            // Resturn a failure OperationResult in order to go back up in the call stack
            return new OperationResult<List<IHttpWorker>>()
            {
                Exception = credidentials.Exception,
                Status = OperationResultStatus.Failed,
                Message = credidentials.Message
            };

        }
        
        for (int i = 0; i < workersCount; i++)
        {
            var sherwebWorker = new SherwebWorkers(_configuration);
            sherwebWorker.PrepareWorker(credidentials.Result);
            Workers.Add(
                new SherwebWorkers(
                    _configuration
                )
            );
        }

        return new OperationResult<List<IHttpWorker>>()
        {
            Status = OperationResultStatus.Success,
            Result = Workers,
            Message = $"Successfully created the {Workers.Count} workers."
        };
    }

    public OperationResult<List<IHttpWorker>> CreateWorkers()
    {
        var credidentials = GetCredentials();
        
        if (credidentials.Status != OperationResultStatus.Success)
        {
            
            Logger.LogError($"The program encountered {credidentials.Status.ToString()} " +
                            $"while retrieving the information : {credidentials.Message}"
            );
            
            // Resturn a failure OperationResult in order to go back up in the call stack
            return new OperationResult<List<IHttpWorker>>()
            {
                Exception = credidentials.Exception,
                Status = OperationResultStatus.Failed,
                Message = credidentials.Message
            };

        }
        
        for (int i = 0; i < _numberOfWorkers; i++)
        {
            var sherwebWorker = new SherwebWorkers(_configuration);
            sherwebWorker.PrepareWorker(credidentials.Result);
            Workers.Add(
                new SherwebWorkers(
                    _configuration
                )
            );
        }

        return new OperationResult<List<IHttpWorker>>()
        {
            Status = OperationResultStatus.Success,
            Result = Workers,
            Message = $"Successfully created the {Workers.Count} workers."
        };
    }
    [Obsolete("Create the workers based on pre existing workers as reference. Idk why we'd want that but it's there if we need it")]
    public async Task<OperationResult<List<IHttpWorker>>> CreateWorkers(List<IHttpWorker> workers)
    {
        List<Task> workerTasks = new List<Task>();

        for (int i = 0; i < _numberOfWorkers; i++)
        {
            var worker = new SherwebWorkers(_configuration);
            Workers.Add(worker);
    
            // Run PrepareWorker asynchronously
            var task = worker.PrepareWorker(keys);
            workerTasks.Add(task);
        }

        // Wait for all workers to complete
        await Task.WhenAll(workerTasks);

        // Now continue after all workers are prepared


        return new OperationResult<List<IHttpWorker>>
        {
            Result = workers,
            Message = $"Successfully created the {workers.Count} workers.",
            Status = OperationResultStatus.Success
        };
    }

    public async Task<OperationResult<(DateTime startTime, DateTime endTime, int requestCount)>> GetInformationFromApi(DateTime currentDateTime)
    {
        var tasks = new List<Task>();
        var requestCount = 0;
        var startTime = DateTime.UtcNow;

        // Use a thread-safe counter
        var requestCounter = new AtomicCounter();

        foreach (var worker in Workers)
        {
            tasks.Add(Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // Fetch next request
                        var operationResult = await requestManager.GetNextRequest();
                        var (hasRequest, request, shouldStop) = operationResult.Result;
                        if (shouldStop)
                        {
                            Console.WriteLine("No more requests will be available, stopping worker.");
                            break;
                        }

                        if (hasRequest && request != null)
                        {
                            var result = await worker.SendRequest(request, 3000);
                            Console.WriteLine($"Processed request: {result}");

                            // Increment the counter
                            requestCounter.Increment();
                        }
                        else
                        {
                            // No request available right now — retry after a small delay
                            await Task.Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing request: {ex.Message}");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        var endTime = DateTime.UtcNow;

        return new OperationResult<(DateTime startTime, DateTime endTime, int requestCount)>
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully made all the calls to Sherweb.",
            Result = (startTime, endTime, requestCounter.Value)
        };
    }

    /// <summary>
    /// Thread safe counter, will be used to count asynchronously all the methods processed
    /// </summary>
    public class AtomicCounter
    {
        private int _value;

        public int Value => _value;

        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }
    }

}