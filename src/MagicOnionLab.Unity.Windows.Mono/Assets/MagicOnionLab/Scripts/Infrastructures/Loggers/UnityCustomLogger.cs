using System;
using UnityEngine;

namespace MagicOnionLab.Unity.Infrastructures.Loggers
{
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
}
