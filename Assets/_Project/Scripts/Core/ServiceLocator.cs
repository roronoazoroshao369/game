using System;
using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Lookup nhanh cho MonoBehaviour singleton-ish (PlayerStats, Inventory, RealmSystem, ...).
    /// Cache lazy: lần đầu Get() rơi xuống FindObjectOfType, các lần sau hit dictionary.
    /// Thay thế <c>FindObjectOfType&lt;T&gt;()</c> rải rác → giảm O(n) scan toàn scene.
    ///
    /// Service tự register trong Awake (nhanh hơn) hoặc bỏ trống (auto-fallback).
    /// Khi scene reload / GO destroy, Unity fake-null detect và Get() refresh tự động.
    ///
    /// Pattern:
    ///   void Awake() { ServiceLocator.Register&lt;PlayerStats&gt;(this); }
    ///   void OnDestroy() { ServiceLocator.Unregister&lt;PlayerStats&gt;(this); }
    ///
    /// Consumer:
    ///   var stats = ServiceLocator.Get&lt;PlayerStats&gt;();
    /// </summary>
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, MonoBehaviour> cache = new();

        /// <summary>
        /// Đăng ký service vào registry. Gọi trong <c>Awake</c>.
        /// Idempotent: register đè instance cũ.
        /// </summary>
        public static void Register<T>(T instance) where T : MonoBehaviour
        {
            if (instance == null) return;
            cache[typeof(T)] = instance;
        }

        /// <summary>
        /// Hủy đăng ký nếu instance hiện cache khớp (tránh ghi đè bởi instance khác).
        /// Gọi trong <c>OnDestroy</c>.
        /// </summary>
        public static void Unregister<T>(T instance) where T : MonoBehaviour
        {
            if (cache.TryGetValue(typeof(T), out var existing) && existing == (MonoBehaviour)instance)
            {
                cache.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Lookup service. Hit cache → O(1). Miss → fallback FindObjectOfType + cache.
        /// Trả null nếu không tồn tại.
        /// Detect Unity fake-null (object destroyed): refresh cache transparent.
        /// </summary>
        public static T Get<T>() where T : MonoBehaviour
        {
            var key = typeof(T);
            if (cache.TryGetValue(key, out var existing))
            {
                // Unity overloads == để check destroyed; existing != null khớp cả "real null" và "fake null".
                if (existing != null) return existing as T;
                cache.Remove(key);
            }
            var found = UnityEngine.Object.FindObjectOfType<T>();
            if (found != null) cache[key] = found;
            return found;
        }

        /// <summary>
        /// Wipe toàn bộ cache. Gọi khi scene reload / "Bắt đầu mới" / test teardown.
        /// </summary>
        public static void ClearAll() => cache.Clear();

        /// <summary>
        /// Số entry hiện tại (cho test / debug).
        /// </summary>
        public static int Count => cache.Count;
    }
}
