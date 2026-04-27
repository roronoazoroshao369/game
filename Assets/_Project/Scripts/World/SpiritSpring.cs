using UnityEngine;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Player;
using WildernessCultivation.UI;

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

        void Awake()
        {
            // Beacon cyan cho minimap — kì ngộ rực rỡ.
            if (GetComponent<MinimapBeacon>() == null)
            {
                var beacon = gameObject.AddComponent<MinimapBeacon>();
                beacon.beaconColor = new Color(0.3f, 0.85f, 1f, 0.95f);
                beacon.childName = "SpiritSpringBeacon";
                beacon.scale = 2.5f;
            }
        }

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
            // Detach VFX trước khi destroy parent để particle có thời gian render —
            // nếu để chung parent, Destroy(gameObject) sẽ kill ParticleSystem ngay
            // end-of-frame, particle chưa kịp hiện.
            if (auraVfx != null)
            {
                auraVfx.transform.SetParent(null, worldPositionStays: true);
                auraVfx.Play();
                if (Application.isPlaying)
                {
                    var main = auraVfx.main;
                    Destroy(auraVfx.gameObject, main.duration + main.startLifetime.constantMax);
                }
                else
                {
                    // EditMode: Destroy delayed không có scheduler chạy → cleanup ngay
                    // để VFX không lơ lửng orphan sau khi spring biến mất.
                    DestroyImmediate(auraVfx.gameObject);
                }
            }
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
            return true;
        }
    }
}
