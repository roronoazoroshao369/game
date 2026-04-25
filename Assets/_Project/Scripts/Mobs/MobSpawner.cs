using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Mobs
{
    /// <summary>
    /// Spawn quái theo cap. Day spawn rabbit/wolf, Night thêm foxSpirit.
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

        Vector2 worldMin, worldMax;
        TimeManager time;
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
        }

        void Update()
        {
            alive.RemoveAll(g => g == null);
            if (Time.time < nextSpawnAt) return;
            nextSpawnAt = Time.time + spawnInterval;

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
