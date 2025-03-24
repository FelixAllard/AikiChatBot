using Microsoft.IdentityModel.Tokens;

namespace AikiDataBuilder.Model.SystemResponse;
using Microsoft.Extensions.Logging;
/// <summary>
/// This class will be used in order to have efficient returns for functions 
/// </summary>
/// <typeparam name="T">The type of the result field</typeparam>

public class OperationResult<T>
{
    private string _message;
    /// <summary>
    /// Optional Constructor
    /// </summary>
    /// <param name="message">A custom message to send with the method</param>
    /// <param name="status">The status using the <see cref="AikiDataBuilder.Model.SystemResponse.OperationResultStatus"/></param>
    /// <param name="exception">If an error occured, we put the error inside</param>
    /// <param name="result">The result which will be of the class given type</param>
    public OperationResult(
        string message = "", 
        OperationResultStatus status = OperationResultStatus.Success, 
        Exception exception = null, 
        T result = default
    )
    {
        Message = message;
        Status = status;
        Exception = exception;
        Result = result;
        
        
    }
    /// <summary>
    /// A message we can attach to the result
    /// Currently configured the get to Pass in Logs
    /// into the console so every function can easily be tracked
    /// </summary>

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            if (_message.IsNullOrEmpty())
                return;
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // You can add other providers like Debug, File, etc.
            });

            ILogger<OperationResult<T>> logger = factory.CreateLogger<OperationResult<T>>();
            if(Exception == null)
                logger.LogInformation(_message);
            else
                logger.LogError(_message);
        }
    }
    /// <summary>
    /// Status
    /// </summary>
    public OperationResultStatus Status { get; set; }
    /// <summary>
    /// If any errors, can be fed into this field
    /// </summary>
    public Exception Exception { get; set; }
    /// <summary>
    /// The result of the function if any
    /// </summary>
    public T Result { get; set; }

    /// <summary>
    /// Basic ToString Function
    /// </summary>
    /// <returns>Returns all the fields in string format</returns>
    public string ToString()
    {
        return $"{nameof(Message)}: {Message}, {nameof(Status)}: {Status}, {nameof(Exception)}: {Exception.Message}";
    }
}