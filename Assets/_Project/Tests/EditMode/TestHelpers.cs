using System.Reflection;
using UnityEngine;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Helpers for EditMode tests. Unity does NOT call MonoBehaviour
    /// lifecycle methods (Awake / OnEnable / Start) on AddComponent in EditMode
    /// — only in PlayMode. Tests that depend on Awake-time wiring (singleton
    /// registration, slot init, event subscription) must invoke them
    /// explicitly via these helpers.
    /// </summary>
    internal static class TestHelpers
    {
        const BindingFlags AllInstance =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>Reflectively invoke a private/public lifecycle method on a
        /// MonoBehaviour. Walks up the type chain so protected base members
        /// (e.g. <c>MobBase.Awake</c>) are reached. No-op if the method is
        /// missing or the target is null.</summary>
        public static void InvokeLifecycle(MonoBehaviour mb, string methodName)
        {
            if (mb == null) return;
            for (var t = mb.GetType(); t != null && t != typeof(MonoBehaviour); t = t.BaseType)
            {
                var m = t.GetMethod(methodName, AllInstance,
                    null, System.Type.EmptyTypes, null);
                if (m != null)
                {
                    m.Invoke(mb, null);
                    return;
                }
            }
        }

        /// <summary>Convenience: invoke <c>Awake</c> followed by <c>OnEnable</c>
        /// — the common boot sequence callers want.</summary>
        public static void Boot(MonoBehaviour mb)
        {
            InvokeLifecycle(mb, "Awake");
            InvokeLifecycle(mb, "OnEnable");
        }

        /// <summary>Boot many components in order (each gets Awake → OnEnable).</summary>
        public static void Boot(params MonoBehaviour[] components)
        {
            foreach (var c in components) Boot(c);
        }
    }
}
