using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;

namespace WildernessCultivation.World
{
    /// <summary>
    /// Linh Tuyền — spawn sau day 7 trong run survival hard-core. Player tương tác
    /// (uống) → trigger <see cref="AwakeningSystem.TryAwaken"/>. Bất kể outcome
    /// (Phàm/Tạp/Đơn/Thiên), spring được consume — không thử lại.
    ///
    /// Eligibility check qua <see cref="AwakeningSystem.CheckEligibility"/>: nếu chưa
    /// đủ điều kiện cần (day < 7 / HP thấp / sanity thấp) → Interact trả false +
    /// spring KHÔNG bị consume (player chưa đủ duyên thật sự).
    /// </summary>
    public class SpiritSpring : MonoBehaviour, IInteractable
    {
        [Header("Visual / SFX")]
        public ParticleSystem auraVfx;

        public string InteractLabel => "Linh Tuyền — Uống Khai Mở";

        public bool CanInteract(GameObject actor)
        {
            if (actor == null) return false;
            var sys = actor.GetComponentInParent<AwakeningSystem>();
            if (sys == null) return false;
            return sys.CheckEligibility() == AwakenEligibility.Eligible;
        }

        public bool Interact(GameObject actor)
        {
            if (actor == null) return false;
            var sys = actor.GetComponentInParent<AwakeningSystem>();
            if (sys == null) return false;

            if (!sys.TryAwaken(out _))
            {
                // Chưa đủ điều kiện → giữ spring lại.
                return false;
            }

            // Đủ điều kiện → roll xong (kể cả fail), spring tự destroy.
            if (auraVfx != null) auraVfx.Play();
            Destroy(gameObject);
            return true;
        }
    }
}
