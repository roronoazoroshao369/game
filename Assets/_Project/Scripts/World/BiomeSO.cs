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

        [Header("Resource prefabs")]
        public GameObject treePrefab;
        public GameObject rockPrefab;
        public GameObject grassBushPrefab;
        public GameObject waterSpringPrefab;

        [Header("Density [0..1]")]
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
        /// </summary>
        [Header("Extra resource nodes (plants, minerals)")]
        public ExtraNode[] extraNodes;

        [System.Serializable]
        public struct ExtraNode
        {
            public GameObject prefab;
            [Range(0f, 1f)] public float density;
        }
    }
}
