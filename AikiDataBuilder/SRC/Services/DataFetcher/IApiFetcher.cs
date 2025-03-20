using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.Workers;

namespace AikiDataBuilder.Services.DataFetcher;

/// <summary>
/// The IApiFetcher will be responsible for holding operation to a single api. It will use Workers in order to accelerate calls to the API to be as fast as possible
/// </summary>
public interface IApiFetcher
{
    
        
    /// <summary>
    /// The different workers which will be responsible for the api calls
    /// </summary>
    public List<IHttpWorker> Workers { get; set; }



    /// <summary>
    /// Will bring in dependencies without the need of dependency injection
    /// </summary>
    /// <returns>
    /// Whether it was successful or not
    /// </returns>
    public OperationResult<bool> Init();

    /// <summary>
    /// Will create the inital credidentials which will be used by the callers for various informations
    /// </summary>
    /// <returns>Returns a OperationResult holding a dictionary of all the keys</returns>
    public OperationResult<Dictionary<string, string>> GetCredentials();

    /// <summary>
    /// This function will be asynchronous. It will store the workers in the Workers variable and return them
    /// </summary>
    /// <param name="workersCount">How many workers will be created. It should be equivalent of the maximum number of concurrent request that can be done at the same time</param>
    /// <returns>A Task Operation Result which will hold the list of workers</returns>
    /// <remarks>
    /// <b>Deprecated:</b> Use <see cref="CreateWorkers()"/> instead. This is because we want each ApiFetchers to have their own numbers of workers
    /// </remarks>
    [Obsolete]
    public OperationResult<List<IHttpWorker>> CreateWorkers(int workersCount);
    /// <summary>
    /// This function will be asynchronous. It will store the workers in  the Workers variable and return them
    /// </summary>
    /// <returns>A Task Operation Result which will hold the list of workers</returns>
    public OperationResult<List<IHttpWorker>> CreateWorkers();
    /// <summary>
    /// Not a used function, but will be responsible for creating workers using already existing Workers
    /// </summary>
    /// <param name="workers">The List of workers that will be used</param>
    /// <returns>A Task Operation Result which will hold the list of workers</returns>
    public Task<OperationResult<List<IHttpWorker>>> CreateWorkers(List<IHttpWorker> workers);
    /// <summary>
    /// This function will do all the operations for a single api,
    /// managing all the workers in order to accelerate api calls
    /// </summary>
    /// <param name="currentDateTime">The current Date Time in order to know if we should retrieve from the database or get from the external API</param>
    /// <returns>Returns all the information in JSON Format</returns>
    public Task<OperationResult<(DateTime startTime, DateTime endTime, int requestCount)>> GetInformationFromApi(DateTime currentDateTime);
}