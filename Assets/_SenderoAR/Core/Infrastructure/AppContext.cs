using System;
using UnityEngine;

namespace KendalLab.SenderoAR.Core.Infrastructure
{
    public static class AppContext
    {
        public static bool IsReady { get; private set; }

        public static void Initialize()
        {
            if (IsReady)
            {
                Debug.LogWarning("[AppContext] Initialize() called twice. Ignored.");
                return;
            }
            IsReady = true;
            Debug.Log("[AppContext] Initialized.");
        }

        public static void Reset()
        {
            IsReady = false;
            Debug.Log("[AppContext] Reset.");
        }

        internal static void EnsureReady(string callerName)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException(
                    $"[AppContext] Access from '{callerName}' rejected: " +
                    "Initialize() has not been called yet.");
            }
        }
    }
}