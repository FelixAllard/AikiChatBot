using System.Diagnostics;
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

    private Type? _instantiaserType;
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
        
        Status = status;
        Exception = exception;
        Result = result;
        //Message must be last so that Exception are registered inside the class before the logging happens
        Message = message;
        
        
        
        var stackTrace = new StackTrace();
        var callingFrame = stackTrace.GetFrame(1); // 1 means the direct caller
        var callingMethod = callingFrame?.GetMethod();
        var callingClass = callingMethod?.DeclaringType;
        _instantiaserType = callingClass;
        
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
            
            LogMessage(_message);
        }
    }
    /// <summary>
    /// Status
    /// </summary>
    public OperationResultStatus Status { get; set; }
    /// <summary>
    /// If any errors, can be fed into this field
    /// </summary>
    private Exception _exception;
    public Exception Exception
    {
        get => _exception;
        set
        {
            _exception = value;
            if(_exception != null)
                LogMessage($"Encountered {_exception.ToString() } : {_exception.Message}", LogLevel.Error); // Log again when Exception is updated
        }
    }
    /// <summary>
    /// The result of the function if any
    /// </summary>
    public T Result { get; set; }


    private void LogMessage(string message, LogLevel logLevel = LogLevel.Information)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // You can add other providers like Debug, File, etc.
        });
        ;
        if (_instantiaserType != null)
        {
            ILogger logger = factory.CreateLogger(_instantiaserType);
            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.LogCritical(_message);
                    break;
                case LogLevel.Error:
                    logger.LogError(_message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(_message);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(_message);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(_message);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(_message);
                    break;
                default:
                    logger.LogInformation(_message);
                    break;
            }
        }
        else
        {
            ILogger<OperationResult<T>> logger = factory.CreateLogger<OperationResult<T>>();
            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.LogCritical(_message);
                    break;
                case LogLevel.Error:
                    logger.LogError(_message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(_message);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(_message);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(_message);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(_message);
                    break;
                default:
                    logger.LogInformation(_message);
                    break;
            }
        }
             

        
    }

    /// <summary>
    /// Basic ToString Function
    /// </summary>
    /// <returns>Returns all the fields in string format</returns>
    public string ToString()
    {
        return $"{nameof(Message)}: {Message}, {nameof(Status)}: {Status}, {nameof(Exception)}: {Exception.Message}";
    }
}