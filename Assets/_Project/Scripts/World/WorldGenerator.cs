using UnityEngine;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.World
{
    /// <summary>
    /// World gen procedural. Hai chế độ:
    ///  1. <see cref="biomes"/> rỗng → fallback dùng các field "legacy" (tree/rock/grass/waterSpring + densities)
    ///     để giữ tương thích với scene đã setup từ MVP scaffold.
    ///  2. <see cref="biomes"/> có ≥1 <see cref="BiomeSO"/> → mỗi tile chọn biome theo Perlin "biome map"
    ///     (vd 2 biome chia 50/50 theo selectionRange [0,0.5] và [0.5,1]) rồi mới rải tài nguyên theo
    ///     density của biome đó.
    ///
    /// Player có thể query <see cref="BiomeAt(Vector3)"/> để lấy biome hiện tại (dùng cho ambient SAN damage,
    /// spirit energy multiplier, …).
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        [Header("World")]
        public int seed = 12345;
        public Vector2Int size = new(100, 100);
        public Transform contentParent;

        [Header("Biomes (cách dùng mới — bỏ trống để fallback prefab legacy)")]
        public BiomeSO[] biomes;
        [Tooltip("Scale của Perlin map dùng để chọn biome (giá trị nhỏ → vùng biome rộng).")]
        public float biomeNoiseScale = 0.025f;

        [Header("Legacy single-biome fallback (chỉ dùng khi 'biomes' rỗng)")]
        public GameObject groundPrefab;
        public GameObject treePrefab;
        public GameObject rockPrefab;
        public GameObject grassBushPrefab;
        [Tooltip("Suối / vũng nước nhỏ — uống tại chỗ.")]
        public GameObject waterSpringPrefab;
        [Range(0f, 1f)] public float treeDensity = 0.10f;
        [Range(0f, 1f)] public float rockDensity = 0.04f;
        [Range(0f, 1f)] public float grassDensity = 0.20f;
        [Range(0f, 1f)] public float waterDensity = 0.005f;

        [Header("Mob spawner")]
        public MobSpawner mobSpawner;

        [Header("Player spawn (giữa map)")]
        public Transform player;

        const float ResourceNoiseScale = 0.12f;

        public static WorldGenerator Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("[WorldGenerator] >1 instance trong scene — chỉ giữ instance mới nhất.");
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

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
            bool useBiomes = biomes != null && biomes.Length > 0;

            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                float n = Mathf.PerlinNoise((x + seed) * ResourceNoiseScale, (y + seed) * ResourceNoiseScale);
                BiomeSO biome = useBiomes ? PickBiomeFor(x, y) : null;

                GameObject ground = biome != null ? biome.groundPrefab : groundPrefab;
                if (ground != null)
                    Instantiate(ground, new Vector3(x + 0.5f, y + 0.5f, 0f), Quaternion.identity, contentParent);

                GameObject tree = biome != null ? biome.treePrefab : treePrefab;
                GameObject rock = biome != null ? biome.rockPrefab : rockPrefab;
                GameObject grass = biome != null ? biome.grassBushPrefab : grassBushPrefab;
                GameObject water = biome != null ? biome.waterSpringPrefab : waterSpringPrefab;

                float dTree = biome != null ? biome.treeDensity : treeDensity;
                float dRock = biome != null ? biome.rockDensity : rockDensity;
                float dGrass = biome != null ? biome.grassDensity : grassDensity;
                float dWater = biome != null ? biome.waterDensity : waterDensity;

                if (water != null && n < 0.15f && Random.value < dWater)
                    Spawn(water, x, y);
                else if (tree != null && n > 0.6f && Random.value < dTree)
                    Spawn(tree, x, y);
                else if (rock != null && n < 0.25f && Random.value < dRock)
                    Spawn(rock, x, y);
                else if (grass != null && Random.value < dGrass)
                    Spawn(grass, x, y);
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

        /// <summary>
        /// Chọn biome cho tile (x,y) bằng Perlin "biome map" riêng. Biome đầu tiên có
        /// selectionRange chứa giá trị Perlin được chọn. Nếu không có biome nào match → trả về biome[0].
        /// </summary>
        BiomeSO PickBiomeFor(int x, int y)
        {
            if (biomes == null || biomes.Length == 0) return null;
            float v = Mathf.PerlinNoise((x + seed * 0.7f) * biomeNoiseScale,
                                         (y - seed * 0.7f) * biomeNoiseScale);
            foreach (var b in biomes)
            {
                if (b == null) continue;
                if (v >= b.selectionRange.x && v <= b.selectionRange.y) return b;
            }
            return biomes[0];
        }

        /// <summary>Trả về biome ở vị trí world (làm tròn về tile). Null nếu chưa setup biome list.</summary>
        public BiomeSO BiomeAt(Vector3 worldPos)
        {
            if (biomes == null || biomes.Length == 0) return null;
            int x = Mathf.Clamp((int)worldPos.x, 0, Mathf.Max(0, size.x - 1));
            int y = Mathf.Clamp((int)worldPos.y, 0, Mathf.Max(0, size.y - 1));
            return PickBiomeFor(x, y);
        }
    }
}
