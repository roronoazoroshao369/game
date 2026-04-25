using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Player.Status;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Hàng icon hiệu ứng trạng thái lên player. Mỗi entry hiển thị icon + thời gian còn lại.
    /// Cách dùng: gắn 1 prefab "StatusIcon" có Image (gọi tên child "Icon") + Text (child "Timer").
    /// Layout cha tự xử lý (HorizontalLayoutGroup chẳng hạn).
    /// </summary>
    public class StatusEffectBarUI : MonoBehaviour
    {
        public StatusEffectManager manager;
        public RectTransform iconContainer;
        public GameObject iconPrefab;

        readonly List<GameObject> spawned = new();

        void Start()
        {
            if (manager == null) manager = FindObjectOfType<StatusEffectManager>();
            if (manager != null) manager.OnEffectsChanged += Rebuild;
            Rebuild();
        }

        void Update()
        {
            // Update timer text mỗi frame mà không Rebuild list
            if (manager == null) return;
            int n = Mathf.Min(spawned.Count, manager.Active.Count);
            for (int i = 0; i < n; i++)
            {
                var a = manager.Active[i];
                var go = spawned[i];
                if (go == null) continue;
                var t = go.GetComponentInChildren<Text>();
                if (t != null)
                {
                    if (a.endsAt <= 0f) t.text = "∞";
                    else t.text = Mathf.CeilToInt(a.RemainingSec).ToString();
                }
            }
        }

        void OnDestroy()
        {
            if (manager != null) manager.OnEffectsChanged -= Rebuild;
        }

        void Rebuild()
        {
            ClearSpawned();
            if (manager == null || iconContainer == null || iconPrefab == null) return;
            foreach (var a in manager.Active)
            {
                if (a == null || a.effect == null) continue;
                var go = Instantiate(iconPrefab, iconContainer);
                var img = go.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = a.effect.icon;
                    img.color = a.effect.tintColor;
                }
                spawned.Add(go);
            }
        }

        void ClearSpawned()
        {
            foreach (var go in spawned) if (go != null) Destroy(go);
            spawned.Clear();
        }
    }
}
