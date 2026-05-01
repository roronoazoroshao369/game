using UnityEngine;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Tile decoration "cỏ" — small green sprite spawn từ <see cref="WorldGenerator"/> grass-tile pass.
    /// Mob (rabbit) tìm + ăn → call <see cref="Eat"/> để Mark cell harvested + Destroy GO.
    /// State persist qua <c>WorldSaveData.harvestedGrassCells</c>: chunk reload / save reload
    /// vẫn skip cell đã eat.
    ///
    /// KHÔNG phải <see cref="ResourceNode"/> (no HP / drops / Interact). Chỉ là cosmetic +
    /// food source cho herbivore mob.
    /// </summary>
    public class GrassTile : MonoBehaviour
    {
        /// <summary>World cell coord (x, y). Set bởi <c>WorldGenerator.GenerateCellAt</c> sau Instantiate.</summary>
        public Vector2Int cellCoord;

        /// <summary>True khi tile đã bị ăn — guard double-eat từ multiple eaters cùng tick.</summary>
        public bool IsEaten { get; private set; }

        /// <summary>
        /// Mob ăn tile này. Mark world generator → cell skip respawn. Destroy GO.
        /// Idempotent: gọi nhiều lần chỉ effective lần đầu.
        /// </summary>
        public void Eat()
        {
            if (IsEaten) return;
            IsEaten = true;

            var wg = FindWorldGenerator();
            if (wg != null) wg.MarkGrassHarvested(cellCoord.x, cellCoord.y);

            Destroy(gameObject);
        }

        WorldGenerator FindWorldGenerator()
        {
            // Parent chain — chunk parent's parent là contentParent's parent (= scene root or wg).
            // FindObjectOfType acceptable ở event-time (Eat one-shot, không phải Update path).
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<WorldGenerator>();
#else
            return Object.FindObjectOfType<WorldGenerator>();
#endif
        }
    }
}
