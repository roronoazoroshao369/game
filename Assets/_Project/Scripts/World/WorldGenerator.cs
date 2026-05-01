using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.Core;
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
    public class WorldGenerator : MonoBehaviour, ISaveable
    {
        // ===== R6 ISaveable =====
        public string SaveKey => "World/Seed";
        public int Order => 0; // Seed trước tất cả — TimeManager (5) cũng không depend seed.

        public void CaptureState(SaveData data)
        {
            if (data == null) return;
            data.world ??= new WorldSaveData();
            data.world.seed = seed;
        }

        public void RestoreState(SaveData data)
        {
            if (data?.world == null) return;
            if (data.world.seed != 0) seed = data.world.seed;
        }

        [Header("World")]
        public int seed = 12345;
        public Vector2Int size = new(100, 100);
        public Transform contentParent;

        [Header("Biomes (cách dùng mới — bỏ trống để fallback prefab legacy)")]
        public BiomeSO[] biomes;
        [Tooltip("Scale của Perlin map dùng để chọn biome (giá trị nhỏ → vùng biome rộng).")]
        public float biomeNoiseScale = 0.025f;

        [Header("Ground rendering (Tilemap — preferred)")]
        [Tooltip("Tilemap dùng để vẽ nền. Khi set: Generate() gọi SetTile per cell (1 SpriteRenderer batch) thay vì Instantiate per tile — tiết kiệm hàng ngàn GameObject ở map lớn. Bỏ trống để fallback per-tile Instantiate (legacy).")]
        public Tilemap groundTilemap;
        [Tooltip("Tile asset fallback cho Tilemap khi 'biomes' rỗng (chế độ legacy single-biome).")]
        public TileBase legacyGroundTile;

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

        [Header("Permadeath / Awakening props")]
        [Tooltip("Tombstone prefab (Mộ Phần). Cần có Tombstone component + sprite tone tối để hiện trên minimap. Null → skip spawn.")]
        public GameObject tombstonePrefab;
        [Tooltip("Item database để Tombstone giải mã itemId khi player drain.")]
        public ItemDatabase itemDatabase;
        [Tooltip("Spirit Spring prefab — spawn 1 instance per world (kì ngộ Linh Tuyền). Null → skip.")]
        public GameObject spiritSpringPrefab;

        const float ResourceNoiseScale = 0.12f;

        public static WorldGenerator Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("[WorldGenerator] >1 instance trong scene — chỉ giữ instance mới nhất.");
            Instance = this;
            ServiceLocator.Register<WorldGenerator>(this);
        }

        void OnDestroy()
        {
            ServiceLocator.Unregister<WorldGenerator>(this);
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            SaveRegistry.RegisterSaveable(this);
        }

        void OnDisable()
        {
            SaveRegistry.UnregisterSaveable(this);
        }

        void Start()
        {
            GenerateNow();

            if (player != null)
                player.position = new Vector3(size.x * 0.5f, size.y * 0.5f, 0f);

            if (mobSpawner != null) mobSpawner.SetupBounds(Vector2.zero, size);

            SpawnTombstones();
            SpawnSpiritSpring();
        }

        /// <summary>
        /// Public hook để EditMode test gọi Generate mà không kích hoạt full Start (mob spawner /
        /// tombstone / player teleport — không relevant cho test ground rendering).
        /// Cũng dùng được trong runtime nếu cần regen world (nhớ clear contentParent + tilemap trước).
        /// </summary>
        public void GenerateNow()
        {
            if (contentParent == null) contentParent = transform;
            Random.InitState(seed);
            Generate();
        }

        /// <summary>
        /// Spawn tombstones từ Graveyard data tại vị trí random (deterministic theo
        /// seed + tombstone id). Mỗi tombstone là 1 prop interactable; player drain →
        /// remove khỏi graveyard.json. Cap 10 từ Graveyard layer.
        /// </summary>
        void SpawnTombstones()
        {
            if (tombstonePrefab == null) return;
            var data = Graveyard.Load();
            if (data == null || data.tombstones == null || data.tombstones.Count == 0) return;

            foreach (var entry in data.tombstones)
            {
                if (entry == null || entry.items == null || entry.items.Count == 0) continue;
                var pos = DeterministicPositionFor(entry.id ?? "tomb_unknown");
                var go = Instantiate(tombstonePrefab, pos, Quaternion.identity, contentParent);
                var ts = go.GetComponent<Tombstone>();
                if (ts == null) ts = go.AddComponent<Tombstone>();
                ts.Initialize(entry, itemDatabase);
            }
        }

        void SpawnSpiritSpring()
        {
            if (spiritSpringPrefab == null) return;
            var pos = DeterministicPositionFor($"spring_{seed}");
            Instantiate(spiritSpringPrefab, pos, Quaternion.identity, contentParent);
        }

        /// <summary>
        /// Vị trí random nhưng deterministic theo (seed, key). Dùng hash đơn giản
        /// của key để stable across reloads — nếu player chưa drain và scene re-Generate,
        /// tombstone xuất hiện đúng vị trí cũ.
        /// </summary>
        Vector3 DeterministicPositionFor(string key)
        {
            int h = StableHash(key);
            // Tránh biên: padding 4 ô khỏi mép map. Tránh trung tâm (player spawn) padding 6.
            int padding = 4;
            int rx = ModPositive(h, Mathf.Max(1, size.x - 2 * padding)) + padding;
            int ry = ModPositive(h / 31, Mathf.Max(1, size.y - 2 * padding)) + padding;
            Vector2 pos = new(rx + 0.5f, ry + 0.5f);
            Vector2 mid = new(size.x * 0.5f, size.y * 0.5f);
            // Push ra khỏi center 6 ô để player spawn xong không thấy ngay.
            if (Vector2.Distance(pos, mid) < 6f)
            {
                Vector2 dir = (pos - mid).sqrMagnitude > 0.001f ? (pos - mid).normalized : Vector2.right;
                pos = mid + dir * 6f;
            }
            return new Vector3(pos.x, pos.y, 0f);
        }

        static int StableHash(string s)
        {
            unchecked
            {
                int h = 23;
                if (s != null)
                    foreach (char c in s) h = h * 31 + c;
                return h;
            }
        }

        static int ModPositive(int v, int m)
        {
            int r = v % m;
            return r < 0 ? r + m : r;
        }

        void Generate()
        {
            bool useBiomes = biomes != null && biomes.Length > 0;

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    float n = Mathf.PerlinNoise((x + seed) * ResourceNoiseScale, (y + seed) * ResourceNoiseScale);
                    BiomeSO biome = useBiomes ? PickBiomeFor(x, y) : null;

                    TileBase tile = PickGroundTile(biome, x, y);
                    if (groundTilemap != null && tile != null)
                    {
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                    else
                    {
                        GameObject ground = biome != null ? biome.groundPrefab : groundPrefab;
                        if (ground != null)
                            Instantiate(ground, new Vector3(x + 0.5f, y + 0.5f, 0f), Quaternion.identity, contentParent);
                    }

                    GameObject tree = biome != null ? biome.treePrefab : treePrefab;
                    GameObject rock = biome != null ? biome.rockPrefab : rockPrefab;
                    GameObject grass = biome != null ? biome.grassBushPrefab : grassBushPrefab;
                    GameObject water = biome != null ? biome.waterSpringPrefab : waterSpringPrefab;

                    float dTree = biome != null ? biome.treeDensity : treeDensity;
                    float dRock = biome != null ? biome.rockDensity : rockDensity;
                    float dGrass = biome != null ? biome.grassDensity : grassDensity;
                    float dWater = biome != null ? biome.waterDensity : waterDensity;

                    bool spawned = false;
                    if (water != null && n < 0.15f && Random.value < dWater)
                    { Spawn(water, x, y); spawned = true; }
                    else if (tree != null && n > 0.6f && Random.value < dTree)
                    { Spawn(tree, x, y); spawned = true; }
                    else if (rock != null && n < 0.25f && Random.value < dRock)
                    { Spawn(rock, x, y); spawned = true; }
                    else if (grass != null && Random.value < dGrass)
                    { Spawn(grass, x, y); spawned = true; }

                    // Extra nodes (linh thảo / mineral) — pass riêng, có thể overlap nếu
                    // tile chưa lấp. Tối đa 1 extra/tile để tránh dày đặc. Tôn trọng Perlin
                    // band khi configured (mặc định 0..0 = no constraint).
                    if (!spawned && biome != null && biome.extraNodes != null)
                    {
                        foreach (var en in biome.extraNodes)
                        {
                            if (en.prefab == null || en.density <= 0f) continue;
                            if (!InPerlinBand(n, en.perlinMin, en.perlinMax)) continue;
                            if (Random.value < en.density)
                            {
                                Spawn(en.prefab, x, y);
                                spawned = true;
                                break;
                            }
                        }
                    }

                    // Decoration pass — visual only, chỉ spawn khi tile chưa có resource.
                    // Decoration không phải resource (không có Interact / harvest), không
                    // kế thừa block tile resource — đứng trên ground bình thường.
                    if (!spawned && biome != null && biome.decorations != null)
                    {
                        foreach (var d in biome.decorations)
                        {
                            if (d.prefab == null || d.density <= 0f) continue;
                            if (!InPerlinBand(n, d.perlinMin, d.perlinMax)) continue;
                            if (Random.value < d.density)
                            {
                                Spawn(d.prefab, x, y);
                                break;
                            }
                        }
                    }
                }
        }

        /// <summary>
        /// Pick ground tile cho cell (x,y). Ưu tiên biome.groundTileVariants nếu có (deterministic
        /// hash) → biome.groundTile → legacyGroundTile. Variant pick stable across regenerate cùng seed.
        /// </summary>
        public TileBase PickGroundTile(BiomeSO biome, int x, int y)
        {
            if (biome != null && biome.groundTileVariants != null && biome.groundTileVariants.Length > 0)
            {
                int idx = (int)(VariantHash(seed, x, y) % (uint)biome.groundTileVariants.Length);
                var picked = biome.groundTileVariants[idx];
                if (picked != null) return picked;
                // Variant slot null → fallback groundTile (don't crash on partial config).
            }
            return biome != null ? biome.groundTile : legacyGroundTile;
        }

        /// <summary>
        /// Avalanche hash cho (seed, x, y) → uint với bit mixing tốt. Thay cho công thức cũ
        /// `seed*A ^ x*B ^ y*C` — công thức cũ có 3 multiplier đều odd, làm LSB của kết quả
        /// = (x+y) parity → `idx % N` (N nhỏ) alias với checkerboard, các đường chéo nhận
        /// cùng variant index. Khi 4 variants có brightness/tone hơi khác → sọc chéo lặp đều
        /// nhìn rất "ô caro" (xem screenshot bug report).
        ///
        /// Dùng "lowbias32" finalizer (https://nullprogram.com/blog/2018/07/31/) — pass
        /// statistical bit-mixing tests, ~2ns/call. Verified Python simulate trên 64×64 grid:
        /// distribution uniform ±3%, adjacent same-variant rate 0.25 (target 0.25), parity
        /// correlation 0.50 (target 0.50, was 1.00 = checkerboard hoàn hảo trước fix).
        /// </summary>
        public static uint VariantHash(int seed, int x, int y)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 0x85ebca6bu;
                h ^= (uint)y * 0xc2b2ae35u;
                h ^= h >> 16;
                h *= 0x7feb352du;
                h ^= h >> 15;
                h *= 0x846ca68bu;
                h ^= h >> 16;
                return h;
            }
        }

        /// <summary>
        /// True nếu Perlin value n nằm trong band [min,max]. Mặc định min=max=0 → no constraint
        /// (treat as 0..1). max=0 cũng được coi là no upper bound (giữ backward compat với
        /// ExtraNode cũ chưa set perlinMin/Max).
        /// </summary>
        public static bool InPerlinBand(float n, float min, float max)
        {
            if (min <= 0f && max <= 0f) return true;          // unset → no constraint
            if (max <= min) return n >= min;                  // upper unset → only lower bound
            return n >= min && n <= max;
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
