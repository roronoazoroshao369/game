using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Spawn quái theo cap. Day spawn rabbit/wolf, Night thêm foxSpirit.
    /// Khi player đứng trong "deep dark" (đêm + ngoài tất cả LightSource) → spawn nhanh hơn
    /// (multiplier <see cref="darknessSpawnRateMult"/>).
    /// </summary>
    public class MobSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct SpawnEntry
        {
            public GameObject prefab;
            public int dayCap;
            public int nightCap;
        }

        public SpawnEntry[] entries;
        public float spawnInterval = 5f;
        public Transform parent;

        [Tooltip("Khi player đứng trong deep dark (đêm + ngoài LightSource) → spawnInterval / mult.")]
        public float darknessSpawnRateMult = 2f;

        Vector2 worldMin, worldMax;
        TimeManager time;
        PlayerStats playerRef;
        readonly List<GameObject> alive = new();
        float nextSpawnAt;

        public void SetupBounds(Vector2 min, Vector2 max)
        {
            worldMin = min; worldMax = max;
        }

        void Start()
        {
            time = GameManager.Instance != null ? GameManager.Instance.timeManager : FindObjectOfType<TimeManager>();
            if (parent == null) parent = transform;
            playerRef = FindObjectOfType<PlayerStats>();
        }

        bool IsPlayerInDeepDark()
        {
            if (time == null || !time.isNight || playerRef == null) return false;
            return !LightSource.AnyLightAt(playerRef.transform.position);
        }

        void Update()
        {
            alive.RemoveAll(g => g == null);
            if (Time.time < nextSpawnAt) return;
            float interval = IsPlayerInDeepDark() ? spawnInterval / Mathf.Max(0.1f, darknessSpawnRateMult) : spawnInterval;
            nextSpawnAt = Time.time + interval;

            bool isNight = time != null && time.isNight;
            foreach (var e in entries)
            {
                int cap = isNight ? e.nightCap : e.dayCap;
                int currentOfType = CountOfPrefab(e.prefab);
                if (currentOfType < cap)
                {
                    var pos = new Vector3(
                        Random.Range(worldMin.x, worldMax.x),
                        Random.Range(worldMin.y, worldMax.y),
                        0f);
                    var go = Instantiate(e.prefab, pos, Quaternion.identity, parent);
                    alive.Add(go);
                }
            }
        }

        int CountOfPrefab(GameObject prefab)
        {
            int c = 0;
            foreach (var g in alive)
                if (g != null && g.name.Contains(prefab.name)) c++;
            return c;
        }
    }
}
