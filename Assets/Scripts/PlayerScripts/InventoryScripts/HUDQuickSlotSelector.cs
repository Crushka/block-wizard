using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any persistent GameObject (e.g. the Player or HUD root).
///
/// Responsibilities:
///   • Mouse-wheel → cycle the active HUD quick-slot (0-3)
///   • R key        → activate the item in the active slot (e.g. drink a potion)
///   • Drives HUDSlotActiveIndicator on each HUD mirror slot to show selection
///
/// Inspector setup:
///   • slotCount      — must match hudMirrorSlots.Count in InventorySlotManager (default 4)
///   • playerHealth   — reference to PlayerHealth for healing
/// </summary>
public class HUDQuickSlotSelector : MonoBehaviour
{
    // ── Singleton (optional, for external access) ─────────────────────────────
    public static HUDQuickSlotSelector Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Количество быстрых слотов (должно совпадать с числом HUD Mirror Slots)")]
    [SerializeField] private int slotCount = 4;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerController playerController;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private int _activeIndex = 0;
    public int ActiveIndex => _activeIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Update()
    {
        HandleScrollWheel();
        HandleUseKey();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleScrollWheel()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll == 0f) return;

        int direction = scroll > 0f ? -1 : 1;          // scroll up → предыдущий слот
        SetActiveSlot((_activeIndex + direction + slotCount) % slotCount);
    }

    private void HandleUseKey()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.rKey.wasPressedThisFrame) return;

        UseActiveSlot();
    }

    // ── Slot selection ────────────────────────────────────────────────────────

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;

        // Снимаем подсветку с предыдущего
        UpdateIndicator(_activeIndex, false);

        _activeIndex = index;

        // Включаем подсветку нового
        UpdateIndicator(_activeIndex, true);
    }

    // ── Use item ──────────────────────────────────────────────────────────────

    private void UseActiveSlot()
    {
        InventorySlotManager mgr = InventorySlotManager.Instance;
        if (mgr == null) return;

        // Реальные данные — в quick-slot инвентаря, не в HUD-зеркале
        InventoryItem item = mgr.GetQuickSlotItem(_activeIndex);
        if (item == null)
        {
            Debug.Log($"[QuickSlot] Слот {_activeIndex} пуст.");
            return;
        }

        // Активируем предмет
        Item itemData = item.ItemData;
        if (itemData is IQuickSlotable usable)
        {
            // Передаём PlayerHealth в зелье, если оно поддерживает
            if (itemData is Potion potion)
            {
                potion.SetPlayerHealth(playerHealth);
                potion.SetPlayerController(playerController);
            }

            usable.Activate();
        }
        else
        {
            Debug.Log($"[QuickSlot] Предмет '{itemData.id}' не является IQuickSlotable.");
            return;
        }

        // Уменьшаем стак — если дошло до 0, слот очищается автоматически
        mgr.ConsumeFromQuickSlot(_activeIndex);
    }

    // ── Indicator helper ──────────────────────────────────────────────────────

    private void UpdateIndicator(int index, bool active)
    {
        InventorySlotManager mgr = InventorySlotManager.Instance;
        if (mgr == null) return;

        InventorySlot mirror = mgr.GetHUDMirror(index);
        if (mirror == null) return;

        HUDSlotActiveIndicator indicator = mirror.GetComponent<HUDSlotActiveIndicator>();
        if (indicator == null) return;

        if (active) indicator.Activate();
        else indicator.Deactivate();
    }

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Выставляем начальную подсветку
        UpdateIndicator(_activeIndex, true);
    }
}