using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Net;
using System.Text.Json;
using AikiDataBuilder.Database;
using AikiDataBuilder.Exceptions;
using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher.Model;
using AikiDataBuilder.Services.SherwebFetcher.Requests;
using AikiDataBuilder.Services.Workers;
using Microsoft.AspNetCore.Http.HttpResults;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;


namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebRequestManager : IRequestManager
{
    public ConcurrentQueue<Request> Requests { get; set; }
    public int AvailableWorkers { get; set; } = 10000;
    private IConfiguration Configuration { get; }
    private SherwebDbContext SherwebDbContext { get; }
    
    private IServiceProvider ServiceProvider { get; }
    private Dictionary<string, string> Keys { get; }
    private CancellationTokenSource? _cts;
    private Task? _backgroundTask;
    private IHttpClientFactory _httpClientFactory;
    private bool noMoreRequest = false;
    private List<int> activeWorkers = new List<int>();
    
    
    private string bearerToken;
    public bool AllActiveWorkers
    {
        get
        {
            if (0==AvailableWorkers)
            {
                //Will Continue the queue creation
                _waitHandle.Set();
                return true;
            }
            return false;
        }
    }

    
    private bool awaitingOtherWorkers = false;


    public SherwebRequestManager(
        IConfiguration config, 
        SherwebDbContext sherwebDbContext,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider
    )
    {
        _requestQueue = new ConcurrentQueue<Request>();
        Configuration = config;
        SherwebDbContext = sherwebDbContext;
        Keys = GetCredentials().Result;
        _httpClientFactory = httpClientFactory;
        ServiceProvider = serviceProvider;

        var tokenCreation = ResetAuthorizationToken();
        
        if (tokenCreation != null)
        {
            var (token, result) = tokenCreation.Result.Result;
            if (result == false)
            {
                throw new Exception("Invalid token");
            }
        }
            
        
        //This will start the task Population process
        StartTask();
        _waitHandle.Set();
    }
    
    /// <summary>
    /// Constructor For test DO NOT USE
    /// </summary>
    /// <param name="config"></param>
    /// <param name="httpClientFactory"></param>
    /// <exception cref="Exception"></exception>
    public SherwebRequestManager(
        IConfiguration config, 
        IHttpClientFactory httpClientFactory
    )
    {
        _requestQueue = new ConcurrentQueue<Request>();
        Configuration = config;
        _httpClientFactory = httpClientFactory;
        Keys = GetCredentials().Result;
    }
    /// <summary>
    /// This function  is responsible to start the async loop task that will add all the request to the
    /// Queue
    /// </summary>
    public void StartTask()
    {
        if (_backgroundTask != null && !_backgroundTask.IsCompleted)
            return; // Prevent multiple task instances

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _backgroundTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await QueueNextRequests();
                    await Task.Delay(1000, token); // Prevent CPU overload
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }, token);
    }
    /// <summary>
    /// This can be used to stop the loop, thought I won't be using it, it can be practical
    /// </summary>
    public void StopTask()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
    /// <summary>
    /// Gets all the credentials and create a dictionary of it
    /// </summary>
    /// <returns>The created dictionary which associates the names with </returns>
    public OperationResult<Dictionary<string, string>> GetCredentials()
    {
        
        string baseUrl = Configuration["SherwebCredentials:BaseUrl"];
        string subscriptionKey = Configuration["SherwebCredentials:SubscriptionKey"];
        string clientId = Configuration["SherwebCredentials:ClientId"];
        string clientSecret = Configuration["SherwebCredentials:ClientSecret"];
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
                    { "ClientSecret", clientSecret },
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
                { "ClientSecret", clientSecret },
            },
            Message = "All the information was available! Successfully fetched from AppSettings.json file.",
            Status = OperationResultStatus.Success
        };
    }


    public OperationResult<bool> ActivateWorker(int clientId,int numberOfWorkers = 1)
    {
        if(activeWorkers.FirstOrDefault(x => x == clientId)!=null)
            return new OperationResult<bool>()
            {
                Message = $"Worker already active",
                Result = AllActiveWorkers,
                Exception = null,
                Status = OperationResultStatus.Success
            };
        activeWorkers.Add(clientId);
        AvailableWorkers = activeWorkers.Count;
        return new OperationResult<bool>()
        {
            Message = $"Worker count {numberOfWorkers}",
            Result = AllActiveWorkers,
            Exception = null,
            Status = OperationResultStatus.Success
        };
    }

    public OperationResult<bool> ReturnWorker(int clientId, int numberOfWorkers = 1)
    {
        if(activeWorkers.FirstOrDefault(x => x == clientId)==null)
            return new OperationResult<bool>()
            {
                Message = $"Worker not active",
                Result = AllActiveWorkers,
                Exception = null,
                Status = OperationResultStatus.Success
            };
        activeWorkers.Remove(clientId);
        AvailableWorkers = activeWorkers.Count;
        return new OperationResult<bool>()
        {
            Message = $"Worker count {numberOfWorkers}",
            Result = AllActiveWorkers,
            Exception = null,
            Status = OperationResultStatus.Success
        };
    }
    /// <summary>
    /// Will hold the request to doa
    /// </summary>
    private readonly ConcurrentQueue<Request> _requestQueue = new ConcurrentQueue<Request>();
    public async Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> GetNextRequest()
    {
        //TODO never returns a should stop and as such will never result in a End
        if (_requestQueue.IsEmpty)
        {
            if (noMoreRequest)
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Message = "Finished getting requests",
                    Result = (
                        false,
                        null,
                        true
                    ),
                    Status = OperationResultStatus.PartialSuccess,
                    Exception = new NoRequestLeftException("No request available.")
                });
        }
            
        if (_requestQueue.TryDequeue(out var request))
        {
            
            return new OperationResult<(
                bool hasRequest, 
                Request? request, 
                bool shouldStop
                )>()
            {
                Result = (true, request, false),
                Message = "Request retrieved successfully",
                Status = OperationResultStatus.Success
            };
        }
        return new OperationResult<(
            bool hasRequest, 
            Request? request, 
            bool shouldStop
        )>()
        {
            Message = "No requests available",
            Result = (false, null, false),
            Status = OperationResultStatus.Success
        };
    }

    /// <summary>
    /// Will reset the authorization Token
    /// </summary>
    /// <returns> Returns a string which is the authorization token and a bool which is whether it worker or not</returns>
    public async Task<OperationResult<(string, bool)>> ResetAuthorizationToken()
    {
        HttpClient authorizationRequest = _httpClientFactory.CreateClient();
        authorizationRequest.Timeout = TimeSpan.FromSeconds(1000);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sherweb.com/auth/oidc/connect/token");

        var collection = new List<KeyValuePair<string, string>>
        {
            new("client_id", Keys["ClientId"]),
            new("client_secret", Keys["ClientSecret"]),
            new("scope", "service-provider"),
            new("grant_type", "client_credentials")
        };

        request.Content = new FormUrlEncodedContent(collection);

        var response = await authorizationRequest.SendAsync(request);
        string jsonResponse = await response.Content.ReadAsStringAsync(); // Read response content

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new OperationResult<(string, bool)>
                {
                    Result = (tokenResponse?.Access_Token, false),
                    Message = $"Received a non-200 OK response ({response.StatusCode}): {jsonResponse}", // Include API response
                    Status = OperationResultStatus.Failed
                };
            }

            if (tokenResponse == null)
            {
                return new OperationResult<(string, bool)>
                {
                    Result = (null, false),
                    Message = $"Failed to retrieve access token. API Response: {jsonResponse}", // Include API response
                    Status = OperationResultStatus.Failed
                };
            }

            bearerToken = tokenResponse.Access_Token;
            return new OperationResult<(string, bool)>
            {
                Result = (tokenResponse.Access_Token, true),
                Message = "Access token retrieved successfully",
                Status = OperationResultStatus.Success
            };
        }
        else
        {
            return new OperationResult<(string, bool)>
            {
                Message = $"Failed to retrieve access token. API Response: {jsonResponse}", // Include API response
                Status = OperationResultStatus.Critical,
                Exception = new Exception($"Failed to retrieve token: {response.StatusCode}")
            };
        }
    }
    
    /// <summary>
    /// Will reset the authorization Token
    /// </summary>
    /// <returns> Returns a string which is the authorization token and a bool which is whether it worker or not</returns>
    public async Task<OperationResult<(string, bool)>> ResetAuthorizationTokenForDistributorApi()
    {
        HttpClient authorizationRequest = _httpClientFactory.CreateClient();
        authorizationRequest.Timeout = TimeSpan.FromSeconds(1000);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sherweb.com/auth/oidc/connect/token");

        var collection = new List<KeyValuePair<string, string>>
        {
            new("client_id", Keys["ClientId"]),
            new("client_secret", Keys["ClientSecret"]),
            new("scope", "distributor"),
            new("grant_type", "client_credentials")
        };

        request.Content = new FormUrlEncodedContent(collection);

        var response = await authorizationRequest.SendAsync(request);
        string jsonResponse = await response.Content.ReadAsStringAsync(); // Read response content

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new OperationResult<(string, bool)>
                {
                    Result = (tokenResponse?.Access_Token, false),
                    Message = $"Received a non-200 OK response ({response.StatusCode}): {jsonResponse}", // Include API response
                    Status = OperationResultStatus.Failed
                };
            }

            if (tokenResponse == null)
            {
                return new OperationResult<(string, bool)>
                {
                    Result = (null, false),
                    Message = $"Failed to retrieve access token. API Response: {jsonResponse}", // Include API response
                    Status = OperationResultStatus.Failed
                };
            }
            return new OperationResult<(string, bool)>
            {
                Result = (tokenResponse.Access_Token, true),
                Message = "Access token retrieved successfully",
                Status = OperationResultStatus.Success
            };
        }
        else
        {
            return new OperationResult<(string, bool)>
            {
                Message = $"Failed to retrieve access token. API Response: {jsonResponse}", // Include API response
                Status = OperationResultStatus.Critical,
                Exception = new Exception($"Failed to retrieve token: {response.StatusCode}")
            };
        }
    }


    public async Task<OperationResult<bool>> ReturnRequest(Request request, RequestReturnJustification shouldStop, bool critical = false)
    {
        var currentReturnedRequest = request;
        try
        {
            if (shouldStop == RequestReturnJustification.UnAuthorized)
            {
                var authorizationRequest = await ResetAuthorizationToken();
                if(authorizationRequest.Status != OperationResultStatus.Success)
                    return await Task.FromResult(new OperationResult<bool>()
                    {
                        Message = "We were not able to refresh the token and " +
                                  "as such the program results in failure",
                        Status = OperationResultStatus.Critical,
                        Exception = authorizationRequest.Exception,
                        Result = false
                    });
                var (bearerToken, successStatus) = authorizationRequest.Result;
                if(!successStatus)
                    return await Task.FromResult(new OperationResult<bool>()
                    {
                        Message = "The authorization request was a failure",
                        Status = OperationResultStatus.Critical,
                        Exception = authorizationRequest.Exception,
                        Result = false
                    });
                currentReturnedRequest.SetBearerToken(bearerToken);
                //Return to queue
                AddRequest(currentReturnedRequest);
                return await Task.FromResult(new OperationResult<bool>()
                {
                    Message = "Successfully put the request back in the queue and recreated authorization token",
                    Status = OperationResultStatus.Critical,
                    Result = true
                });
            }
            else if(shouldStop == RequestReturnJustification.NotFound)
            {
                return await Task.FromResult(new OperationResult<bool>()
                {
                    Message = "We were not able to find the endpoint specified",
                    Status = OperationResultStatus.Critical,
                    Exception = new ("The endpoint we searched for does not exist, application Compromised"),
                    Result = false
                });
            }
            else if (shouldStop == RequestReturnJustification.InternalServerError)
            {
                return await Task.FromResult(new OperationResult<bool>()
                {
                    Message = "The response server failer to execute the request",
                    Status = OperationResultStatus.Critical,
                    Exception = new ("The endpoint we searched for does not exist, application Compromised"),
                    Result = false
                });
            }
            else
            {
                return await Task.FromResult(new OperationResult<bool>()
                {
                    Message = "A Unprepared exception lead to program runtime to fail",
                    Status = OperationResultStatus.Critical,
                    Exception = new ("We will not be able to run the request and the program will stop"),
                    Result = false
                });
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // Example: Adding new requests safely
    public void AddRequest(Request request)
    {
        _requestQueue.Enqueue(request);
    }


    private int[] retrievalStep = [0, 0];
    private bool waitForAllWorkers = false;
    /// <summary>
    /// The task Scheduler trigger lever
    /// </summary>
    private readonly ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);

    public async Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> QueueNextRequests()
    {
        while (true)
        {
            //Will block the infinite loop until all workers are available
            _waitHandle.Wait();
            _waitHandle.Reset();

                // Here we will put all the steps inside a switch
                switch (retrievalStep[0])
                {
                    case 0:
                        var getAllCustomerRequest = InstantiateRequestClass<GetAllCustomers>().Result;
                        getAllCustomerRequest.SetBearerToken(bearerToken);
                        getAllCustomerRequest.SetHeaderVariables(new Dictionary<string, string>()
                        {
                            {
                                "Ocp-Apim-Subscription-Key",
                                Keys[
                                    "SubscriptionKey"
                                ]
                            }
                        });
                        AddRequest(getAllCustomerRequest);
                        break;
                    case 1:
                        //NOT USED RIGHT NOW SO WE SKIP
                        /*retrievalStep[0] = 2;
                        goto case 3;*/

                        var allCustomersReceivableCharges = SherwebDbContext.Customers.ToList();
                        foreach (var customer in allCustomersReceivableCharges)
                        {
                            var getReceivableChargesRequest = InstantiateRequestClass<GetReceivableCharges>().Result;
                            getReceivableChargesRequest.SetBearerToken(bearerToken);
                            getReceivableChargesRequest.SetHeaderVariables(new Dictionary<string, string>()
                            {
                                {
                                    "Ocp-Apim-Subscription-Key",
                                    Keys[
                                        "SubscriptionKey"
                                    ]
                                }
                            });
                            getReceivableChargesRequest.SetRouteVariables(new Dictionary<string, string>()
                            {
                                {
                                    "{{customerId}}",
                                    customer.Id.ToString()
                                }
                            });
                            AddRequest(getReceivableChargesRequest);
                        }
                        break;
                    case 2:
                        SherwebDbContext.Subscriptions.RemoveRange(SherwebDbContext.Subscriptions);
                        SherwebDbContext.SubscriptionFees.RemoveRange(SherwebDbContext.SubscriptionFees);
                        SherwebDbContext.CommitmentTerms.RemoveRange(SherwebDbContext.CommitmentTerms);
                        SherwebDbContext.RenewalConfigurations.RemoveRange(SherwebDbContext.RenewalConfigurations);
                        SherwebDbContext.CommittedMinimalQuantities.RemoveRange(SherwebDbContext.CommittedMinimalQuantities);
                        SherwebDbContext.SaveChanges();
                        var allCustomersSubscriptions = SherwebDbContext.Customers.ToList();
                        int xTest = 0;
                        foreach (var customer in allCustomersSubscriptions)
                        {
                            Console.WriteLine(xTest++);
                            var getSubscriptions = InstantiateRequestClass<GetSubscriptions>().Result;
                            getSubscriptions.SetBearerToken(bearerToken);
                            getSubscriptions.SetHeaderVariables(new Dictionary<string, string>()
                            {
                                {
                                    "Ocp-Apim-Subscription-Key",
                                    Keys[
                                        "SubscriptionKey"
                                    ]
                                }
                            });
                            getSubscriptions.SetRouteVariables(new Dictionary<string, string>()
                            {
                                {
                                    "{{customerId}}",
                                    customer.Id.ToString()
                                }
                            });
                            AddRequest(getSubscriptions);
                        }

                        break;
                    case 3:
                        SherwebDbContext.PayableCharges.RemoveRange(SherwebDbContext.PayableCharges);
                        SherwebDbContext.PayableCharge.RemoveRange(SherwebDbContext.PayableCharge);
                        SherwebDbContext.Tags.RemoveRange(SherwebDbContext.Tags);
                        SherwebDbContext.Taxes.RemoveRange(SherwebDbContext.Taxes);
                        SherwebDbContext.Invoices.RemoveRange(SherwebDbContext.Invoices);
                        SherwebDbContext.Deductions.RemoveRange(SherwebDbContext.Deductions);
                        SherwebDbContext.Fees.RemoveRange(SherwebDbContext.Fees);
                        SherwebDbContext.SaveChanges();

                        var bearerTokenDistributor = ResetAuthorizationTokenForDistributorApi().Result;
                        var (token, result) = bearerTokenDistributor.Result;
                        if (!result)
                            break;
                        
                        var getPayableCharges = InstantiateRequestClass<GetPayableCharges>().Result;
                        getPayableCharges.SetBearerToken(token);
                        getPayableCharges.SetHeaderVariables(new Dictionary<string, string>()
                        {
                            {
                                "Ocp-Apim-Subscription-Key",
                                Keys[
                                    "SubscriptionKey"
                                ]
                            }
                        });
                        AddRequest(getPayableCharges);
                        

                        break;
                    default:
                        //Sigma Male exit
                        noMoreRequest = true;
                        return await Task.FromResult(
                            new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                            {
                                Message = "Not Finding a request to give back",
                                Result = (
                                    true,
                                    null,
                                    true
                                ),
                                Status = OperationResultStatus.Success

                            });
                }

                retrievalStep[0] += 1 ;
        }

    }

    private OperationResult<Request> InstantiateRequestClass<Request>()
    {
        return new OperationResult<Request>()
        {
            Status = OperationResultStatus.Success,
            Message = "Instantiated Request of Type "+ typeof(Request).Name,
            Result = ServiceProvider.GetRequiredService<Request>()
        };
    }

    
    
    
    /*public async Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> QueueNextRequests()
    {
        if(AllWorkersAvailable)
            awaitingOtherWorkers = false;
        if (awaitingOtherWorkers)
            return new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
            {
                Message = $"Currently awaiting all workers to be available",
                Status = OperationResultStatus.PartialSuccess,
                Result = (
                    false, 
                    null, 
                    false
                )
            };
        // Here we will put all the steps inside a switch
        switch (retrievalStep[0])
        {
            case 0:
                var getAllCustomerRequest = new GetAllCustomers(
                    HttpClient,
                    SherwebDbContext
                );
                getAllCustomerRequest.SetBearerToken(bearerToken);
                getAllCustomerRequest.SetHeaderVariables(new Dictionary<string, string>()
                {
                    {
                        "Ocp-Apim-Subscription-Key", 
                        Keys[
                            "SubscriptionKey"
                        ]
                    }
                });
                retrievalStep[0] = 1;
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Result = (
                        true, 
                        getAllCustomerRequest,
                        false
                    ),
                    Message = "Get all customers successfully sent",
                    Status = OperationResultStatus.Success
                });
            default:
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Message = "Not Finding a request to give back",
                    Result =(
                    true,
                    null,
                    true
                    ),
                    Status = OperationResultStatus.Success
                    
                });
        }

        return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
        {
            Message = $"Finished all steps of retrieval from sherweb. Stopping retrieval",
            Status = OperationResultStatus.Success,
            Result = (
                false, 
                null!,
                true
            )
        });
    }*/
    
}