using UnityEngine;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.World
{
    /// <summary>
    /// MVP world gen: 1 biome đồng cỏ, hình chữ nhật cố định. Rải cây/đá/cỏ ngẫu nhiên + spawn quái.
    /// Sử dụng Perlin noise để cluster cây tự nhiên hơn.
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        [Header("World")]
        public int seed = 12345;
        public Vector2Int size = new(100, 100);
        public Transform contentParent;

        [Header("Tile prefab (sprite cỏ nền — optional)")]
        public GameObject groundPrefab;

        [Header("Resource prefabs")]
        public GameObject treePrefab;
        public GameObject rockPrefab;
        public GameObject grassBushPrefab;

        [Header("Density [0..1]")]
        [Range(0f, 1f)] public float treeDensity = 0.10f;
        [Range(0f, 1f)] public float rockDensity = 0.04f;
        [Range(0f, 1f)] public float grassDensity = 0.20f;

        [Header("Mob spawner")]
        public MobSpawner mobSpawner;

        [Header("Player spawn (giữa map)")]
        public Transform player;

        void Start()
        {
            if (contentParent == null) contentParent = transform;
            Random.InitState(seed);
            Generate();

            if (player != null)
                player.position = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);

            if (mobSpawner != null) mobSpawner.SetupBounds(Vector2.zero, size);
        }

        void Generate()
        {
            // Optional: tile nền (nếu null thì bỏ qua, dùng bg color của camera)
            if (groundPrefab != null)
            {
                for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    Instantiate(groundPrefab, new Vector3(x + 0.5f, y + 0.5f, 0f), Quaternion.identity, contentParent);
            }

            float noiseScale = 0.12f;
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                float n = Mathf.PerlinNoise((x + seed) * noiseScale, (y + seed) * noiseScale);

                if (treePrefab != null && n > 0.6f && Random.value < treeDensity)
                    Spawn(treePrefab, x, y);
                else if (rockPrefab != null && n < 0.25f && Random.value < rockDensity)
                    Spawn(rockPrefab, x, y);
                else if (grassBushPrefab != null && Random.value < grassDensity)
                    Spawn(grassBushPrefab, x, y);
            }
        }

        void Spawn(GameObject prefab, int x, int y)
        {
            // tránh spawn ngay trung tâm (chỗ player)
            Vector2 mid = new(size.x * 0.5f, size.y * 0.5f);
            Vector2 p = new(x + 0.5f, y + 0.5f);
            if (Vector2.Distance(p, mid) < 4f) return;

            Instantiate(prefab, new Vector3(p.x, p.y, 0f), Quaternion.identity, contentParent);
        }
    }
}
