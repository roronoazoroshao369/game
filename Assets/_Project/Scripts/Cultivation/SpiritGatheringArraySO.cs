using UnityEngine;
using WildernessCultivation.Combat;
using WildernessCultivation.Player;

namespace WildernessCultivation.Cultivation
{
    /// <summary>
    /// Tụ Linh Trận Pháp — đặt 1 trận pháp tạm thời tại vị trí player. Trong aura, ngồi thiền sẽ
    /// hồi mana / tích XP nhanh hơn (nhân với <see cref="spiritMultiplier"/>).
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Techniques/Spirit Gathering Array", fileName = "Tech_SpiritArray")]
    public class SpiritGatheringArraySO : TechniqueSO
    {
        [Header("Array")]
        public GameObject arrayPrefab;
        public float radius = 3f;
        public float spiritMultiplier = 2f;
        public float lifetime = 30f;

        public override void Cast(PlayerCombat caster)
        {
            Vector3 pos = caster.transform.position;
            GameObject go;
            if (arrayPrefab != null)
            {
                go = Object.Instantiate(arrayPrefab, pos, Quaternion.identity);
            }
            else
            {
                // Fallback: tạo 1 GameObject trần để vẫn có effect dù chưa làm prefab.
                go = new GameObject($"SpiritArray_{Time.frameCount}");
                go.transform.position = pos;
            }
            var arr = go.GetComponent<SpiritArray>() ?? go.AddComponent<SpiritArray>();
            arr.radius = radius;
            arr.spiritMultiplier = spiritMultiplier;
            arr.lifetime = lifetime;

            Debug.Log($"[Skill] Tụ Linh Trận đặt tại {pos}, x{spiritMultiplier} trong {lifetime}s.");
        }
    }
}
