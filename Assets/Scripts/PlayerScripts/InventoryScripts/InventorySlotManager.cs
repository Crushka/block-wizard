using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Central inventory singleton.
///
/// Inspector setup:
///   • inventorySlots  — all regular inventory cell GameObjects
///   • quickSlots      — exactly 4 quick-slot GameObjects (index 0-3)
///   • quickSlotHUDImages — 4 Image components in the player HUD (same order)
///   • inventoryItemPrefab — prefab with InventoryItem + DraggableItem + CanvasGroup
///   • itemIcons       — list of id → Sprite mappings
/// </summary>
public class InventorySlotManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static InventorySlotManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Inventory Slots")]
    [SerializeField] private List<InventorySlot> inventorySlots = new();

    [Header("Quick Slots — панель инвентаря (4 шт, порядок 0-3)")]
    [SerializeField] private List<InventorySlot> quickSlots = new();

    [Header("HUD Mirror Slots — UI игрока (те же префабы, тот же порядок 0-3)")]
    [SerializeField] private List<InventorySlot> hudMirrorSlots = new();

    [Header("Prefab")]
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Item Icons")]
    [SerializeField] private List<ItemIconEntry> itemIcons = new();

    // ── Runtime ───────────────────────────────────────────────────────────────
    private readonly Dictionary<string, InventoryItem> _stackMap = new();

    // ─────────────────────────────────────────────────────────────────────────
    #region Public API

    public void AddItem(Item item)
    {
        if (item == null) return;

        if (_stackMap.TryGetValue(item.id, out InventoryItem existing))
        {
            existing.AddCount(item.amount);
            return;
        }

        InventorySlot freeSlot = inventorySlots.FirstOrDefault(s => s.CurrentItem == null);
        if (freeSlot == null)
        {
            Debug.LogWarning($"[Inventory] No free slot for '{item.id}'.");
            return;
        }

        InventoryItem entry = CreateVisual(item, item.amount);
        freeSlot.PlaceItem(entry);
        _stackMap[item.id] = entry;
    }

    public void CreateNode(ElementType nodeType)
    {
        Debug.Log($"[Inventory] CreateNode: {nodeType}");
    }

    public void ConsumeItem(string itemId, int amount = 1)
    {
        if (!_stackMap.TryGetValue(itemId, out InventoryItem entry)) return;

        entry.AddCount(-amount);
        if (entry.Count > 0) return;

        _stackMap.Remove(itemId);
        FindSlotWith(entry)?.ClearSlot(destroy: true);
    }

    /// <summary>Возвращает HUD-зеркало по индексу быстрого слота.</summary>
    public InventorySlot GetHUDMirror(int index)
    {
        if (index < 0 || index >= hudMirrorSlots.Count) return null;
        return hudMirrorSlots[index];
    }

    /// <summary>Создаёт визуальный клон InventoryItem для HUD (без DraggableItem).</summary>
    public InventoryItem CloneVisual(InventoryItem source)
    {
        InventoryItem clone = CreateVisual(source.ItemData, source.Count);
        return clone;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Private helpers

    private InventoryItem CreateVisual(Item item, int count)
    {
        GameObject go = Instantiate(inventoryItemPrefab);
        InventoryItem iv = go.GetComponent<InventoryItem>();
        iv.Initialise(item, count, GetIcon(item.id));

        if (go.GetComponent<DraggableItem>() == null)
            go.AddComponent<DraggableItem>();

        return iv;
    }

    private Sprite GetIcon(string itemId)
    {
        foreach (var e in itemIcons)
            if (e.id == itemId) return e.sprite;
        return null;
    }

    private InventorySlot FindSlotWith(InventoryItem item)
    {
        foreach (var s in inventorySlots) if (s.CurrentItem == item) return s;
        foreach (var s in quickSlots) if (s.CurrentItem == item) return s;
        return null;
    }

    #endregion

    [System.Serializable]
    public class ItemIconEntry
    {
        public string id;
        public Sprite sprite;
    }
}