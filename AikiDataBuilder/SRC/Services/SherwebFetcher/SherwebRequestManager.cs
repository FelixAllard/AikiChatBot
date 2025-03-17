using System.Collections.Concurrent;

using AikiDataBuilder.Model.SystemResponse;
using AikiDataBuilder.Services.SherwebFetcher.Requests;
using AikiDataBuilder.Services.Workers;


namespace AikiDataBuilder.Services.SherwebFetcher;

public class SherwebRequestManager : IRequestManager
{
    public ConcurrentQueue<Request> Requests { get; set; }
    public int AvailableWorkers { get; set; }
    
    public int MaxWorkers { get; }
    
    public bool AllWorkersAvailable
    {
        get
        {
            if (MaxWorkers==AvailableWorkers)
            {
                return true;
            }
            return false;
        }
    }

    private int[] retrievalStep = [0, 0];
    private bool awaitingOtherWorkers = false;


    public SherwebRequestManager(int workersNumber)
    {
        MaxWorkers = workersNumber;
        AvailableWorkers = MaxWorkers;
    }
    

    public OperationResult<bool> ReturnWorker(int numberOfWorkers = 1)
    {
        AvailableWorkers -= numberOfWorkers;
        return new OperationResult<bool>()
        {
            Message = $"Worker count {numberOfWorkers}",
            Result = AllWorkersAvailable,
            Exception = null,
            Status = OperationResultStatus.Success
        };
    }
    
    public async Task<OperationResult<(bool hasRequest, Request? request, bool shouldStop)>> GetNextRequest()
    {
        if(AllWorkersAvailable)
            awaitingOtherWorkers = false;
        if (awaitingOtherWorkers)
            return new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
            {
                Message = $"Currently awaiting all workers to be available",
                Status = OperationResultStatus.PartialSuccess,
                Result = (false, null, false)
            };
        // Here we will put all the steps inside a switch
        switch (retrievalStep[0])
        {
            case 0:
                return await Task.FromResult(new OperationResult<(bool hasRequest, Request? request, bool shouldStop)>()
                {
                    Result = (
                        true, 
                        new GetAllCustomers(),
                        false)

                });
        }
        
        
        throw new NotImplementedException();
    }
    
}