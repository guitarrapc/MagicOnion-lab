using System;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class UnityNullLogHandler : ILogHandler
    {
        public static ILogHandler Instance = new UnityNullLogHandler();
        public void LogException(Exception exception, UnityEngine.Object context) { }
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) { }
    }
    public class UnityNullLogger : ILogger
    {
        public static ILogger Instance = new UnityNullLogger();

        private static ILogHandler _logHandler = UnityNullLogHandler.Instance;

        public ILogHandler logHandler { get; set; } = _logHandler;
        public bool logEnabled { get; set; } = true;
        public LogType filterLogType { get; set; } = LogType.Log;
        public bool IsLogTypeAllowed(LogType logType) => true;
        public void Log(LogType logType, object message) { }
        public void Log(LogType logType, object message, UnityEngine.Object context) { }
        public void Log(LogType logType, string tag, object message) { }
        public void Log(LogType logType, string tag, object message, UnityEngine.Object context) { }
        public void Log(object message) { }
        public void Log(string tag, object message) { }
        public void Log(string tag, object message, UnityEngine.Object context) { }
        public void LogError(string tag, object message) { }
        public void LogError(string tag, object message, UnityEngine.Object context) { }
        public void LogException(Exception exception) { }
        public void LogException(Exception exception, UnityEngine.Object context) { }
        public void LogFormat(LogType logType, string format, params object[] args) { }
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) { }
        public void LogWarning(string tag, object message) { }
        public void LogWarning(string tag, object message, UnityEngine.Object context) { }
    }

    public class UnityCustomLogger : ILogger
    {
        private ILogger _logger;

        public UnityCustomLogger(bool useUnityLogger)
        {
            if (useUnityLogger)
            {
                _logger = UnityEngine.Debug.unityLogger;
            }
            else
            {
                _logger = UnityNullLogger.Instance;
            }
        }

        public ILogHandler logHandler { get => _logger.logHandler; set => _logger.logHandler = value; }
        public bool logEnabled { get => _logger.logEnabled; set => _logger.logEnabled = value; }
        public LogType filterLogType { get => _logger.filterLogType; set => _logger.filterLogType = value; }

        public bool IsLogTypeAllowed(LogType logType) => _logger.IsLogTypeAllowed(logType);

        public void Log(LogType logType, object message) => _logger.Log(logType, message);

        public void Log(LogType logType, object message, UnityEngine.Object context) => _logger.Log(logType, message, context);

        public void Log(LogType logType, string tag, object message) => _logger.Log(logType, tag, message);

        public void Log(LogType logType, string tag, object message, UnityEngine.Object context) => _logger.Log(logType, tag, message, context);

        public void Log(object message) => _logger.Log(message);

        public void Log(string tag, object message) => _logger.Log(tag, message);

        public void Log(string tag, object message, UnityEngine.Object context) => _logger.Log(tag, message, context);

        public void LogError(string tag, object message) => _logger.LogError(tag, message);

        public void LogError(string tag, object message, UnityEngine.Object context) => _logger.LogError(tag, message, context);

        public void LogException(Exception exception) => _logger.LogException(exception);

        public void LogException(Exception exception, UnityEngine.Object context) => _logger.LogException(exception, context);

        public void LogFormat(LogType logType, string format, params object[] args) => _logger.LogFormat(logType, format, args);

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) => _logger.LogFormat(logType, context, format, args);

        public void LogWarning(string tag, object message) => _logger.LogWarning(tag, message);

        public void LogWarning(string tag, object message, UnityEngine.Object context) => _logger.LogWarning(tag, message, context);
    }

    public static class LoggerExtensions
    {
        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>logger.LogInformation("Processing request from {Address}", address)</example>
        public static void LogInformation(this ILogger logger, string message)
        {
            logger.Log(LogType.Log, message);
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>logger.LogError(exception, "Error while processing request from {Address}", address)</example>
        public static void LogError(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogType.Error, $"{message} {exception.Message} {exception.StackTrace}");
        }
    }
}
