using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Items;
using WildernessCultivation.World;

namespace WildernessCultivation.Player
{
    /// <summary>
    /// Cầm đuốc / lồng đèn — phát aura sáng + warmth quanh player khi bật.
    /// Tốn fuel theo thời gian; hết fuel tự tắt. Toggle bằng phím <see cref="toggleKey"/>
    /// hoặc qua <see cref="SkillButton.Action.ToggleTorch"/>.
    /// </summary>
    public class TorchAction : MonoBehaviour
    {
        [Header("Item & fuel")]
        [Tooltip("ItemSO Torch (loại Tool/Consumable). Cần có ít nhất 1 trong inventory để bật.")]
        public ItemSO torchItem;
        public Inventory inventory;
        [Tooltip("Giây cháy của 1 thanh đuốc.")]
        public float fuelPerTorch = 90f;

        [Header("Aura")]
        public float radius = 4f;
        public float warmthBonus = 8f;

        [Header("Visual")]
        public SpriteRenderer auraRenderer;
        public Color auraColor = new Color(1f, 0.8f, 0.4f, 0.35f);

        [Header("Input (PC)")]
        public KeyCode toggleKey = KeyCode.T;

        public bool IsLit { get; private set; }
        float fuelSeconds = 0f;
        LightSource lightSource;

        void Awake()
        {
            lightSource = GetComponent<LightSource>() ?? gameObject.AddComponent<LightSource>();
            lightSource.radius = radius;
            lightSource.warmthBonus = warmthBonus;
            lightSource.emitting = false;
            UpdateVisual();
            ServiceLocator.Register<TorchAction>(this);
        }

        void OnDestroy() => ServiceLocator.Unregister<TorchAction>(this);

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();

            if (IsLit)
            {
                fuelSeconds = Mathf.Max(0f, fuelSeconds - Time.deltaTime);
                if (fuelSeconds <= 0f) Extinguish();
            }
        }

        public bool Toggle()
        {
            if (IsLit) { Extinguish(); return false; }
            return Light();
        }

        public bool Light()
        {
            if (IsLit) return true;
            if (torchItem == null || inventory == null) return false;
            if (!inventory.TryConsume(torchItem, 1)) return false;
            fuelSeconds = fuelPerTorch;
            IsLit = true;
            if (lightSource != null) lightSource.emitting = true;
            UpdateVisual();
            return true;
        }

        public void Extinguish()
        {
            IsLit = false;
            fuelSeconds = 0f;
            if (lightSource != null) lightSource.emitting = false;
            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (auraRenderer != null)
            {
                auraRenderer.enabled = IsLit;
                auraRenderer.color = auraColor;
                float scale = radius * 2f;
                auraRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
