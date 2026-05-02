using UnityEngine;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// Per-character override cho puppet rig joint positions + side-view occlusion params.
    /// Replace hardcoded constants trong <c>BootstrapWizard.BuildPuppetHierarchy</c>
    /// (shoulderY=0.55, shoulderX=0.30, hipY=-0.55, hipX=0.13, elbowOverlap=0.06,
    /// kneeOverlap=0.10) — vốn được tune cho placeholder colored-rectangle skeleton, KHÔNG khớp
    /// với real art kích thước thật.
    ///
    /// <para>Tạo SO instance per character (Player / Wolf / FoxSpirit / Boss / ...) qua
    /// menu <c>Assets / Create / Wilderness Cultivation / Character Rig Spec</c>. Drop vào
    /// <c>Assets/_Project/Data/CharacterRigs/{id}.asset</c>. BootstrapWizard load by id.
    /// Null/missing → fallback default constants (backwards compatible với existing flow).</para>
    ///
    /// <para>Side-view occlusion params (PR fix "rời rạc"): khi character đang quay E hoặc W,
    /// far limb (cánh tay / chân ở phía xa camera) lùi sortingOrder + scale.x * farLimbScale
    /// để tạo perspective depth — giảm hiệu ứng "paper doll" 2 cặp tay đè cạnh nhau.
    /// Front/back view (N/S) không apply (cả 2 limb cùng visible đối xứng).</para>
    /// </summary>
    [CreateAssetMenu(
        fileName = "CharacterRigSpec",
        menuName = "Wilderness Cultivation/Character Rig Spec",
        order = 100)]
    public class CharacterRigSpec : ScriptableObject
    {
        [Header("Shoulder (arm attach point — local pos relative to spriteRoot)")]
        [Tooltip("Y offset cho shoulder từ spriteRoot center. Default 0.55. Tăng nếu torso art " +
                 "cao hơn placeholder, giảm nếu thấp hơn.")]
        public float shoulderY = 0.55f;
        [Tooltip("X offset cho shoulder (mirrored cho L/R). Default 0.30. Tăng nếu torso art " +
                 "rộng hơn placeholder, giảm nếu hẹp hơn.")]
        public float shoulderX = 0.30f;

        [Header("Hip (leg attach point — local pos relative to spriteRoot)")]
        [Tooltip("Y offset cho hip từ spriteRoot center. Default -0.55.")]
        public float hipY = -0.55f;
        [Tooltip("X offset cho hip (mirrored cho L/R). Default 0.13.")]
        public float hipX = 0.13f;

        [Header("Joint overlap (hide thin top edge của child part dưới parent)")]
        [Tooltip("Forearm overlap up theo armLen — hides thin top taper sau cuff/wrist trim. " +
                 "Default 0.06.")]
        public float elbowOverlap = 0.06f;
        [Tooltip("Shin overlap up theo legLen — hides thin top taper sau trouser hem trim. " +
                 "Default 0.10.")]
        public float kneeOverlap = 0.10f;

        [Header("Side-view occlusion (chỉ apply khi direction = E hoặc W)")]
        [Tooltip("Scale.x cho far limb (limb ở phía xa camera) trong side view. < 1 = thu nhỏ, " +
                 "tạo perspective depth. Default 0.92. Set 1.0 disable scale shrink.")]
        public float farLimbScale = 0.92f;
        [Tooltip("SortingOrder offset cho far limb trong side view (subtract từ near-limb default). " +
                 "Negative = render sau body. Default -2 (far arm/leg z-back so torso che).")]
        public int farLimbSortingOffset = -2;
        [Tooltip("True = enable side-view occlusion logic. False = giữ behavior cũ (cả 2 limb " +
                 "cùng z-order + cùng scale, không depth). Default true.")]
        public bool enableSideViewOcclusion = true;

        /// <summary>
        /// Default values matching legacy hardcoded constants. Caller dùng khi RigSpec asset
        /// missing / null — đảm bảo backwards compatible với existing scenes/prefabs.
        /// </summary>
        public static CharacterRigSpec CreateDefault()
        {
            return CreateInstance<CharacterRigSpec>();
        }

        /// <summary>
        /// Pure helper: tính scale.x cho far limb dựa farLimbScale + flipped direction.
        /// flipSign = -1 (West, spriteRoot.localScale.x đã flip) hoặc +1 (East / N / S).
        /// Trả về scale.x absolute. Caller multiply với flipSign khi gán localScale.x.
        /// </summary>
        public static float ComputeFarLimbScaleX(float farLimbScale, float baseScaleX)
        {
            return Mathf.Abs(baseScaleX) * Mathf.Clamp(farLimbScale, 0.1f, 2f);
        }

        /// <summary>
        /// Pure helper: tính sortingOrder cho far limb. nearOrder = sortingOrder gốc (như
        /// arm thường = sortingOrderBase + 3). Trả về nearOrder + farLimbSortingOffset.
        /// </summary>
        public static int ComputeFarLimbSortingOrder(int nearOrder, int farLimbSortingOffset)
        {
            return nearOrder + farLimbSortingOffset;
        }
    }
}
