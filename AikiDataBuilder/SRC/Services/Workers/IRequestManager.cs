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

    internal int MaxWorkers { get; }
    internal bool AllWorkersAvailable { get;  }
    
    /// <summary>
    /// Will be called whenever a Worker is done with their task
    /// </summary>
    /// <param name="numberOfWorkers">By default, will say that 1 worker finished their job,
    /// if more finished, then it can be specified</param>
    /// <returns>Weather all workers were given back or not</returns>
    public OperationResult<bool> ReturnWorker(int numberOfWorkers = 1);

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
}