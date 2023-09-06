using System;
using UnityEngine;

namespace MagicOnionLab.Unity
{
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
}
