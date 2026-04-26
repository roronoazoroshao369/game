using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WildernessCultivation.Items;
using WildernessCultivation.Player;
using WildernessCultivation.World;

namespace WildernessCultivation.UI
{
    /// <summary>
    /// Panel transfer item player ↔ rương. Tap slot bên player → chuyển sang rương,
    /// tap slot bên rương → chuyển sang player.
    ///
    /// Subscribe vào <see cref="StorageChest.OnAnyChestOpened"/>; ESC hoặc nút Đóng để tắt.
    /// </summary>
    public class StorageChestUI : MonoBehaviour
    {
        [Header("Refs")]
        public Inventory playerInventory;
        public PlayerController playerController;

        [Header("UI")]
        public GameObject panel;
        public Transform chestSlotsParent;
        public Transform playerSlotsParent;
        public GameObject slotPrefab;
        public Button closeButton;
        public KeyCode closeKey = KeyCode.Escape;

        StorageChest currentChest;
        readonly List<InventorySlotUI> chestSlotUIs = new();
        readonly List<InventorySlotUI> playerSlotUIs = new();
        // True khi Open() là cái set MovementLocked = true. Nếu lock đã có sẵn từ
        // hệ khác (FishingAction, DodgeAction) thì Close() KHÔNG release để không
        // ghi đè khoá của hệ kia. MovementLocked là bool dùng chung — chưa có
        // ref-count nên đành dùng cờ ownership cục bộ.
        bool ownsMovementLock;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()
        {
            StorageChest.OnAnyChestOpened += Open;
        }

        void OnDisable()
        {
            StorageChest.OnAnyChestOpened -= Open;
            DetachInventoryListeners();
        }

        void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (playerInventory == null) playerInventory = FindObjectOfType<Inventory>();
            if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        }

        void Update()
        {
            if (panel != null && panel.activeSelf && Input.GetKeyDown(closeKey)) Close();
        }

        void Open(StorageChest chest)
        {
            if (chest == null || playerInventory == null || slotPrefab == null) return;

            // Đóng chest cũ trước khi mở chest mới
            if (currentChest != null) DetachInventoryListeners();
            currentChest = chest;

            if (panel != null) panel.SetActive(true);
            if (playerController != null && !playerController.MovementLocked)
            {
                playerController.MovementLocked = true;
                ownsMovementLock = true;
            }

            EnsureSlotCount(chestSlotUIs, chestSlotsParent,
                chest.ChestInventory.Slots.Count, OnChestSlotClicked);
            EnsureSlotCount(playerSlotUIs, playerSlotsParent,
                playerInventory.Slots.Count, OnPlayerSlotClicked);

            chest.ChestInventory.OnInventoryChanged += RefreshChest;
            playerInventory.OnInventoryChanged += RefreshPlayer;
            RefreshChest();
            RefreshPlayer();
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
            if (ownsMovementLock && playerController != null)
                playerController.MovementLocked = false;
            ownsMovementLock = false;
            DetachInventoryListeners();
            currentChest = null;
        }

        void DetachInventoryListeners()
        {
            if (currentChest != null && currentChest.ChestInventory != null)
                currentChest.ChestInventory.OnInventoryChanged -= RefreshChest;
            if (playerInventory != null) playerInventory.OnInventoryChanged -= RefreshPlayer;
        }

        // Spawn / re-bind slot UIs. Idempotent (gọi nhiều lần không leak GameObjects).
        void EnsureSlotCount(List<InventorySlotUI> list, Transform parent, int n,
            System.Action<int> onClick)
        {
            if (parent == null) return;
            while (list.Count < n)
            {
                var go = Instantiate(slotPrefab, parent);
                var ui = go.GetComponent<InventorySlotUI>();
                ui.slotIndex = list.Count;
                ui.onClick = onClick;
                list.Add(ui);
            }
            // Update click handler + ẩn slot dư (i >= n) để không hiển thị stale data
            // từ chest cũ khi chest mới có ít slot hơn.
            for (int i = 0; i < list.Count; i++)
            {
                list[i].onClick = onClick;
                list[i].gameObject.SetActive(i < n);
            }
        }

        void RefreshChest()
        {
            if (currentChest == null) return;
            var slots = currentChest.ChestInventory.Slots;
            for (int i = 0; i < chestSlotUIs.Count && i < slots.Count; i++)
                chestSlotUIs[i].Bind(slots[i]);
        }

        void RefreshPlayer()
        {
            if (playerInventory == null) return;
            var slots = playerInventory.Slots;
            for (int i = 0; i < playerSlotUIs.Count && i < slots.Count; i++)
                playerSlotUIs[i].Bind(slots[i]);
        }

        void OnPlayerSlotClicked(int idx)
        {
            if (currentChest == null) return;
            playerInventory.TransferSlot(idx, currentChest.ChestInventory);
        }

        void OnChestSlotClicked(int idx)
        {
            if (currentChest == null) return;
            currentChest.ChestInventory.TransferSlot(idx, playerInventory);
        }
    }
}
