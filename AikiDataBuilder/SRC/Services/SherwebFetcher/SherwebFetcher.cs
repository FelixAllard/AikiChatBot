﻿using AikiDataBuilder.Database;
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
    
    private readonly int _numberOfWorkers = 3;
    protected Dictionary<string, string> keys;
    private IRequestManager requestManager;
    private readonly SemaphoreSlim _semaphore;
    
    
    private IConfiguration _configuration;
    private HttpClient _httpClient;
    private IHttpClientFactory _httpClientFactory;
    private SherwebDbContext _sherwebDbContext;
    private ILogger<SherwebFetcher> _logger;
    private IServiceProvider _serviceProvider;
    private IServiceScopeFactory _scope;
    public SherwebFetcher(
        IHttpClientFactory httpClientFactory,
        SherwebDbContext sherwebDbContext,
        IConfiguration configuration,
        SherwebRequestManager requestManager,
        ILogger<SherwebFetcher> logger,
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        Workers = new List<IHttpWorker>();
        _httpClientFactory = httpClientFactory;
        _sherwebDbContext = sherwebDbContext;
        _configuration = configuration;
        this.requestManager = requestManager;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _scope = serviceScopeFactory;
        
        var resultCreationWorkers = CreateWorkers();
        if(resultCreationWorkers.Status!= OperationResultStatus.Success)
            throw new ApplicationException("Failed to create workers For Sherweb Fetcher");
    }

    public OperationResult<bool> Init()
    {
        try
        {
            Workers = new List<IHttpWorker>();
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
                _configuration, 
                _sherwebDbContext,
                _httpClientFactory,
                _serviceProvider
            );
            var resultCreationWorkers = CreateWorkers();
            if(resultCreationWorkers.Status!= OperationResultStatus.Success)
                throw new ApplicationException("Failed to create workers For Sherweb Fetcher");
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
        
        string baseUrl = _configuration["BASE_URL"];
        string subscriptionKey = _configuration["SUBSCRIPTION_KEY"];
        string clientId = _configuration["CLIENT_ID"];
        string clientSecret = _configuration["CLIENT_SECRET"];
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
        Workers = new List<IHttpWorker>();
        var credidentials = GetCredentials();
        
        if (credidentials.Status != OperationResultStatus.Success)
        {
            
            
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
            /*var sherwebWorker = new SherwebWorkers(_configuration);
            sherwebWorker.PrepareWorker(credidentials.Result);*/
            Workers.Add(
                new SherwebWorkers(
                    _configuration,
                    _scope
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
        Workers = new List<IHttpWorker>();
        var credidentials = GetCredentials();
        
        if (credidentials.Status != OperationResultStatus.Success)
        {
            
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
            var sherwebWorker = new SherwebWorkers(_configuration,_scope);
            sherwebWorker.PrepareWorker(credidentials.Result);
            Workers.Add(
                new SherwebWorkers(
                    _configuration,
                    _scope
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
        Workers = new List<IHttpWorker>();
        List<Task> workerTasks = new List<Task>();

        for (int i = 0; i < _numberOfWorkers; i++)
        {
            var worker = new SherwebWorkers(_configuration, _scope);
            worker.WorkerId = i;
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

        Console.WriteLine(Workers.Count);
        foreach (var worker in Workers)
        {
            tasks.Add(Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);
                    try
                    {
                        // Fetch next request
                        var operationResult = await requestManager.GetNextRequest();
                        var (hasRequest, request, shouldStop) = operationResult.Result;
                        if (shouldStop)
                        {
                            _logger.LogWarning("No more requests will be available, stopping worker.");
                            break;
                        }

                        if (hasRequest && request != null)
                        {
                            requestManager.ActivateWorker(worker.WorkerId);
                            var result = await worker.SendRequest(request, 3000);
                            
                            if(result.Status == OperationResultStatus.PartialSuccess)
                                await requestManager.ReturnRequest(
                                    request, 
                                    RequestReturnJustification.UnAuthorized, 
                                    false
                                );
                            
                            _logger.LogInformation($"Processed request: {result}");
                            
                            // Increment the counter
                            requestCounter.Increment();
                        }
                        else
                        {
                            
                            // No request available right now — retry after a small delay
                            requestManager.ReturnWorker(worker.WorkerId);
                            await Task.Delay(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        requestManager.ReturnWorker(worker.WorkerId);
                        _logger.LogError($"Error processing request: {ex.Message}\nStack Trace : {ex.StackTrace}");
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

        /// <summary>
        /// Thread Safe Function
        /// </summary>
        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }
    }

}