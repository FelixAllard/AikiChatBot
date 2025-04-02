using System.Collections.Concurrent;
using AikiDataBuilder.Model.SystemResponse;

namespace AikiDataBuilder.Services.Workers;

/// <summary>
/// I request managers will be responsible to feed into IApiFetchers the different request they will have to do
/// </summary>
public interface IRequestManager
{
    /// <summary>
    /// The Queue of request to get
    /// </summary>
    public ConcurrentQueue<Request> Requests { get; set; }
    
    /// <summary>
    /// The number of Workers owned By the IApiFetcher owning the Request Manager
    /// </summary>
    internal int AvailableWorkers { get; set; }
    /// <summary>
    /// The maximum ammount of workers
    /// </summary>

    internal int MaxWorkers { get; }
    internal bool AllWorkersAvailable { get;  }
    
    public OperationResult<bool> ActivateWorker(int workerId, int numberOfWorkers = 1);
    
    /// <summary>
    /// Will be called whenever a Worker is done with their task
    /// </summary>
    /// <param name="workerId">The ID of the worker we are returning</param>
    /// <param name="numberOfWorkers">By default, will say that 1 worker finished their job,
    /// if more finished, then it can be specified</param>
    /// <returns>Weather all workers were given back or not</returns>
    public OperationResult<bool> ReturnWorker(int workerId,int numberOfWorkers = 1);

    /// <summary>
    /// Gets the next request to feed into a worker.
    /// This method will assume that the request is put inside a worker on exit and as such
    /// will reduce the number of workers by one
    /// </summary>
    /// <remarks>If waiting is necessary, it will hang the answer until all Workers are idle</remarks>
    /// <returns>One or more request depending on how many are needed for idle workers.
    /// <br/>
    /// <br/> <b>HasRequest </b> : Whether a request was attributable
    /// <br/> <b>Request </b> : The Request the worker must execute || <i>NULLABLE</i>
    /// <br/> <b>ShouldStop </b> : If the worker is done
    /// </returns>
    public Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> GetNextRequest();
    
    /// <summary>
    /// This function will be called when a request doesn't go as expected, this way, we will have an easy way to modify
    /// the request and prepare for it's next call. It will also be used for unauthorized.
    /// </summary>
    /// <param name="request">The request to repeat and repair</param>
    /// <param name="shouldStop">What reason for which the request failed</param>
    /// <param name="critical">Should the application stop completly</param>
    /// <returns>Asynchronous Operation Result Bool representing if the application flow will continue</returns>
    public Task<OperationResult<bool>> ReturnRequest(
        Request request, 
        RequestReturnJustification shouldStop, 
        bool critical = false
    );
}