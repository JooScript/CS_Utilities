//using Microsoft.Extensions.Logging;
//using Microsoft.IdentityModel.Tokens;
//using Utils.FileActions;

//namespace Utils.Logging;

///// <summary>
///// Helper class for standardized logging operations.
///// </summary>
///// <typeparam name="Cls">The type whose name will be included in log messages.</typeparam>
//public class LoggingHelper<Cls>
//{
//    private readonly ILogger<Cls> _logger;

//    /// <summary>
//    /// Initializes a new instance of the LogHelper class.
//    /// </summary>
//    /// <param name="logger">The logger instance.</param>
//    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
//    public LoggingHelper(ILogger<Cls> logger)
//    {
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    /// <summary>
//    /// Logs when a method is entered.
//    /// </summary>
//    /// <param name="methodName">The name of the method being entered.</param>
//    //public void LogMethodEntry(string methodName)
//    //{
//    //    if (string.IsNullOrEmpty(methodName))
//    //        throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//    //    if (_logger.IsEnabled(LogLevel.Debug))
//    //        _logger.LogDebug($"Entering {methodName} for type {typeof(Cls).Name}");

//    //}

//    /// <summary>
//    /// Logs a successful operation.
//    /// </summary>
//    /// <param name="methodName">The name of the method.</param>
//    /// <param name="count">The number of items retrieved.</param>
//    //public void LogSuccess(string methodName, int count)
//    //{
//    //    if (string.IsNullOrEmpty(methodName))
//    //        throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//    //    if (_logger.IsEnabled(LogLevel.Debug))
//    //        _logger.LogDebug($"{methodName} successfully retrieved {count} items of type {typeof(Cls).Name}");
//    //}

//    /// <summary>
//    /// Logs when an item is not found.
//    /// </summary>
//    /// <param name="methodName">The name of the method.</param>
//    /// <param name="id">Optional ID of the item not found.</param>
//    public void LogNotFound(string methodName, Guid? id = null)
//    {
//        if (string.IsNullOrEmpty(methodName))
//            throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//        if (_logger.IsEnabled(LogLevel.Debug))
//        {
//            _logger.LogDebug(
//                "{MethodName} did not find {ItemIdentifier} of type {TypeName}",
//                methodName,
//                id.HasValue ? $"item with ID {id.Value}" : "matching item",
//                typeof(Cls).Name);
//        }
//    }

//    /// <summary>
//    /// Logs an error that occurred in a method.
//    /// </summary>
//    /// <param name="methodName">The name of the method.</param>
//    /// <param name="ex">The exception that occurred.</param>
//    public void LogError(string methodName, Exception ex)
//    {
//        if (methodName.IsNullOrEmpty()) throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//        if (ex == null) throw new ArgumentNullException(nameof(ex));

//        string clsName = typeof(Cls).Name;

//        _logger.LogError(ex, $"Error in {methodName} for type {clsName}: {ex.Message}");
//    }

//    /// <summary>
//    /// Logs a warning message.
//    /// </summary>
//    /// <param name="methodName">The name of the method.</param>
//    /// <param name="warningMessage">The warning message.</param>
//    public void LogWarning(string methodName, string warningMessage)
//    {
//        if (string.IsNullOrEmpty(methodName))
//            throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//        if (string.IsNullOrEmpty(warningMessage))
//            throw new ArgumentException("Warning message cannot be null or empty", nameof(warningMessage));

//        _logger.LogWarning($"Warning in {methodName} for type {typeof(Cls).Name}: {warningMessage}");
//    }

//    /// <summary>
//    /// Logs performance metrics for a method.
//    /// </summary>
//    /// <param name="methodName">The name of the method.</param>
//    /// <param name="duration">The duration the method took to execute.</param>
//    public void LogPerformance(string methodName, TimeSpan duration)
//    {
//        if (string.IsNullOrEmpty(methodName))
//            throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

//        if (_logger.IsEnabled(LogLevel.Debug))
//            _logger.LogDebug($"{methodName} for type {typeof(Cls).Name} executed in {duration.TotalMilliseconds}ms");
//    }

//}