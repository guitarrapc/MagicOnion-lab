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
}
