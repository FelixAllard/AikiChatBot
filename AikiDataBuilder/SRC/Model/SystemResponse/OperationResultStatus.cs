namespace AikiDataBuilder.Model.SystemResponse;

/// <summary>
/// This class is used in the Operation Result to know how an operation went
/// </summary>
public enum OperationResultStatus
{
    /// <summary>
    /// Success is when everything went perfectly well
    /// </summary>
    Success,
    /// <summary>
    /// When Things could have gone better, but runtime is not compromised
    /// </summary>
    PartialSuccess,
    /// <summary>
    /// The Program should be terminated
    /// </summary>
    Critical,
    /// <summary>
    /// The process failed
    /// </summary>
    Failed,
}