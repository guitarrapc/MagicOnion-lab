#nullable enable
using MagicOnion;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MagicOnionLab.Unity.Infrastructures
{
    public static class ApplicationHelper
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
