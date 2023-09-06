using System.Threading.Tasks;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                Debug.LogError($"TaskUnhandled {x.Exception.Message} {x.Exception.StackTrace}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
