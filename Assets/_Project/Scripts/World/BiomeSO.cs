using UnityEngine;
using UnityEngine.Tilemaps;
using WildernessCultivation.Mobs;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Định nghĩa 1 biome — bộ prefab tài nguyên + mật độ + (tùy chọn) bảng spawn quái riêng.
    /// Tạo asset: Right-click > Create > WildernessCultivation > Biome.
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Biome", fileName = "Biome_New")]
    public class BiomeSO : ScriptableObject
    {
        [Header("Identity")]
        public string biomeId;          // "grassland" / "spirit_forest" / "death_desert"
        public string displayName;
        [TextArea] public string description;

        [Header("Ground")]
        [Tooltip("Tile nền (sprite cỏ / cát / lá rụng …). Optional. Chỉ dùng khi WorldGenerator.groundTilemap == null (legacy per-tile Instantiate path).")]
        public GameObject groundPrefab;
        [Tooltip("Tile asset cho Tilemap-based ground rendering. Ưu tiên hơn groundPrefab khi WorldGenerator.groundTilemap != null. Render hiệu quả hơn nhiều ở map lớn (1 SpriteRenderer batch thay vì N×M GameObjects).")]
        public TileBase groundTile;
        [Tooltip("Tile variants để phá vỡ pattern lặp khi map lớn. Khi != null & length > 0, WorldGenerator pick deterministic per cell (hash x,y,seed) — overrides groundTile. Empty → fallback groundTile.")]
        public TileBase[] groundTileVariants;

        [Header("Resource prefabs (legacy 4-slot)")]
        public GameObject treePrefab;
        public GameObject rockPrefab;
        public GameObject grassBushPrefab;
        public GameObject waterSpringPrefab;

        [Header("Density [0..1] (legacy 4-slot)")]
        [Range(0f, 1f)] public float treeDensity = 0.10f;
        [Range(0f, 1f)] public float rockDensity = 0.04f;
        [Range(0f, 1f)] public float grassDensity = 0.20f;
        [Range(0f, 1f)] public float waterDensity = 0.005f;

        [Header("Mob spawning (optional override)")]
        [Tooltip("Nếu set, MobSpawner sẽ dùng list này khi player đang trong biome (tổ chức theo cap day/night).")]
        public MobSpawner.SpawnEntry[] mobEntries;

        [Header("Ambience tuning")]
        [Tooltip("Multiplier hồi linh khí khi ngồi thiền trong biome này (mặc định 1.0).")]
        public float spiritEnergyMultiplier = 1.0f;
        [Tooltip("Damage SAN/giây thêm khi đứng trong biome này vào ban đêm — biome 'tử khí' nên cao.")]
        public float ambientNightSanDamage = 0f;

        [Header("Temperature modifier")]
        [Tooltip("Cộng vào nhiệt độ ban ngày của biome (vd Hoang Mạc +25 → giữa trưa rất nóng).")]
        public float temperatureDayOffset = 0f;
        [Tooltip("Cộng vào nhiệt độ ban đêm của biome (vd Hoang Mạc -20 → đêm rất lạnh).")]
        public float temperatureNightOffset = 0f;

        [Header("Selection (Perlin)")]
        [Tooltip("Khoảng [min,max] giá trị Perlin để biome này chiếm. Phải nằm trong [0,1] và 2 biome không nên overlap nhiều.")]
        public Vector2 selectionRange = new(0f, 1f);

        /// <summary>
        /// Plant / linh thảo / mineral phụ — spawn theo từng tile với density riêng.
        /// Mỗi tile lăn 1 lần qua list, gặp prefab match density thì spawn (tối đa 1 / tile).
        /// Dùng cho linh mushroom, berry bush, cactus, death lily, mineral ore …
        /// Optional Perlin band [perlinMin, perlinMax] để giới hạn vùng spawn (vd ore vein chỉ
        /// xuất hiện ở đỉnh núi: perlinMin=0.8, perlinMax=1.0). Default [0,1] = no constraint.
        /// </summary>
        [Header("Extra resource nodes (plants, minerals)")]
        public ExtraNode[] extraNodes;

        [System.Serializable]
        public struct ExtraNode
        {
            public GameObject prefab;
            [Range(0f, 1f)] public float density;
            [Tooltip("Perlin band lower bound (0..1). Default 0 = no constraint.")]
            [Range(0f, 1f)] public float perlinMin;
            [Tooltip("Perlin band upper bound (0..1). Default 0 (treated as 1) = no constraint.")]
            [Range(0f, 1f)] public float perlinMax;
        }

        /// <summary>
        /// Decoration objects — chỉ visual / ambient, không phải resource (không có Interact / harvest).
        /// VD: hoa cỏ dại, nấm phát sáng, xương khô, chum đất vỡ, đèn lồng cũ. Pass riêng sau resource pass:
        /// chỉ spawn nếu tile chưa có resource (nodecoration overlap resource node).
        /// </summary>
        [Header("Decorations (visual only, không phải resource)")]
        public DecorationEntry[] decorations;

        [System.Serializable]
        public struct DecorationEntry
        {
            public GameObject prefab;
            [Range(0f, 1f)] public float density;
            [Tooltip("Perlin band lower bound (0..1). Default 0 = no constraint.")]
            [Range(0f, 1f)] public float perlinMin;
            [Tooltip("Perlin band upper bound (0..1). Default 0 (treated as 1) = no constraint.")]
            [Range(0f, 1f)] public float perlinMax;
        }
    }
}
