using UnityEngine;
using WildernessCultivation.Core;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Holder runtime cho linh căn của player. Gắn vào Player GameObject.
    /// PlayerStats / RealmSystem / PlayerCombat / PlayerController đọc <see cref="Current"/>
    /// để apply multiplier. Save/Load round-trip <see cref="Current"/> theo asset name (rootId).
    /// </summary>
    public class SpiritRoot : MonoBehaviour
    {
        [Tooltip("Linh căn hiện tại. Có thể gán sẵn trong prefab, hoặc roll random từ candidatePool.")]
        public SpiritRootSO current;

        [Tooltip("Pool roll random khi mới tạo nhân vật (current = null + rollOnStart=true).")]
        public SpiritRootSO[] candidatePool;
        public bool rollOnStart = true;

        public SpiritRootSO Current => current;

        void Awake()
        {
            if (current == null && rollOnStart && candidatePool != null && candidatePool.Length > 0)
            {
                current = candidatePool[Random.Range(0, candidatePool.Length)];
                if (current != null)
                    Debug.Log($"[SpiritRoot] Rolled: {current.displayName} ({current.grade}/{current.primaryElement})");
            }
            if (current == null)
            {
                current = SpiritRootSO.CreateDefault();
            }
            ServiceLocator.Register<SpiritRoot>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<SpiritRoot>(this);

        public void SetSpiritRoot(SpiritRootSO root)
        {
            current = root;
            Debug.Log($"[SpiritRoot] Set to: {(root != null ? root.displayName : "null")}");
        }

        // Convenience accessors — trả về 1 nếu không có root.
        public float MaxHPMul => current != null ? current.maxHPMultiplier : 1f;
        public float CarryMul => current != null ? current.carryWeightMultiplier : 1f;
        public float WeaponDamageMul => current != null ? current.weaponDamageMultiplier : 1f;
        public float DurabilityWearMul => current != null ? current.durabilityWearMultiplier : 1f;
        public float HungerDecayMul => current != null ? current.hungerDecayMultiplier : 1f;
        public float ThirstDecayMul => current != null ? current.thirstDecayMultiplier : 1f;
        public float SanityDecayMul => current != null ? current.sanityDecayMultiplier : 1f;
        public float FreezeDamageMul => current != null ? current.freezeDamageMultiplier : 1f;
        public float XpGainMul => current != null ? current.xpGainMultiplier : 1f;
        public float BreakthroughCostMul => current != null ? current.breakthroughCostMultiplier : 1f;

        public float TechniqueAffinityMulFor(SpiritElement element)
        {
            if (current == null || element == SpiritElement.None) return 1f;
            return current.primaryElement == element ? current.techniqueAffinityMultiplier : 1f;
        }
    }
}
