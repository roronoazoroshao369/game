using TMPro;
using UnityEngine;
using WildernessCultivation.Combat;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Listen <see cref="CombatEvents.OnDamageDealt"/> rồi spawn <see cref="DamageNumber"/>
    /// tại vị trí world chuyển qua canvas-space. Đặt component này trên 1 RectTransform
    /// child của Canvas (overlay).
    ///
    /// Yêu cầu: <c>worldCamera</c> được gán để chuyển toạ độ; <c>canvasRect</c> = RectTransform
    /// của Canvas (Screen Space - Overlay) để map screen→canvas local space.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [Header("Refs (auto-find if null)")]
        public Camera worldCamera;
        public RectTransform canvasRect;

        [Header("Style")]
        public int fontSize = 22;
        public Color enemyDamageColor = new Color(1f, 0.85f, 0.2f, 1f);  // số bay từ quái (player gây)
        public Color playerDamageColor = new Color(1f, 0.3f, 0.3f, 1f); // số bay từ player (player bị)
        [Tooltip("Khoảng cách offset Y (pixels) thêm vào sau khi convert từ world.")]
        public float spawnOffsetY = 14f;
        [Tooltip("Jitter ngẫu nhiên trục X để tránh số chồng nhau khi multi-hit.")]
        public float spawnJitterX = 14f;

        [Header("Source detection")]
        [Tooltip("Tag của player để phân biệt damage chỉ player nhận → màu đỏ. Mặc định 'Player'.")]
        public string playerTag = "Player";

        Transform playerTransform;
        const float SAME_AS_PLAYER_THRESHOLD_SQR = 0.04f; // 0.2u — nếu world pos quá gần player, coi như player bị đánh

        void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (canvasRect == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null) canvasRect = (RectTransform)canvas.transform;
            }
            var p = GameObject.FindWithTag(playerTag);
            if (p != null) playerTransform = p.transform;
        }

        void OnEnable() { CombatEvents.OnDamageDealt += OnDamage; }
        void OnDisable() { CombatEvents.OnDamageDealt -= OnDamage; }

        void OnDamage(Vector3 worldPos, float amount, bool isCrit)
        {
            if (canvasRect == null || worldCamera == null) return;
            // Map world → screen → canvas local (Screen Space - Overlay).
            Vector3 screen = worldCamera.WorldToScreenPoint(worldPos);
            if (screen.z < 0f) return; // sau camera, bỏ qua
            screen.y += spawnOffsetY;
            screen.x += Random.Range(-spawnJitterX, spawnJitterX);
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out local);

            // Tạo GO với TMP_Text + DamageNumber.
            var go = new GameObject("DamageNumber",
                typeof(RectTransform), typeof(TextMeshProUGUI), typeof(DamageNumber));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = local;
            rt.sizeDelta = new Vector2(80, 32);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            var dn = go.GetComponent<DamageNumber>();
            bool isPlayer = IsPlayerDamage(worldPos);
            Color c = isPlayer ? playerDamageColor : enemyDamageColor;
            int rounded = Mathf.Max(1, Mathf.RoundToInt(amount));
            dn.Init(rounded.ToString(), c);
        }

        bool IsPlayerDamage(Vector3 worldPos)
        {
            if (playerTransform == null) return false;
            float sqr = ((Vector2)playerTransform.position - (Vector2)worldPos).sqrMagnitude;
            return sqr < SAME_AS_PLAYER_THRESHOLD_SQR;
        }
    }
}
