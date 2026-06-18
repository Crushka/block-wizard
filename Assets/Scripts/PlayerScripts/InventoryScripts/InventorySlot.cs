using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to every inventory cell, every quick-slot cell, and every HUD quick-slot cell.
///
/// isQuickSlot = true       → слот инвентаря (панель инвентаря)
/// isHUDMirror = true       → слот-зеркало в HUD игрока (не принимает drag напрямую)
/// quickSlotIndex (0-3)     → связывает пару quickSlot ↔ HUDMirror
/// </summary>
public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    public bool isQuickSlot = false;
    public bool isHUDMirror = false;
    public int quickSlotIndex = 0;

    [Header("HUD Mirror Settings")]
    [Tooltip("Масштаб иконки в HUD-зеркале относительно оригинала (0.5 = 50%)")]
    [Range(0.1f, 1f)]
    public float hudIconScale = 0.5f;

    public InventoryItem CurrentItem { get; private set; }

    // ── IDropHandler ──────────────────────────────────────────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        // HUD-зеркало не принимает drop напрямую
        if (isHUDMirror) return;

        DraggableItem dragged = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (dragged == null) return;

        InventoryItem incoming = dragged.GetComponent<InventoryItem>();
        if (incoming == null) return;

        InventorySlot fromSlot = dragged.OriginalSlot;

        if (fromSlot == this)
        {
            dragged.ReturnToOriginalSlot();
            return;
        }

        if (CurrentItem == null)
        {
            AcceptItem(incoming, fromSlot);
            return;
        }

        if (CurrentItem.ItemData.id == incoming.ItemData.id)
        {
            CurrentItem.AddCount(incoming.Count);
            fromSlot.ClearSlot(destroy: true);
            SyncHUDMirror(CurrentItem);
            return;
        }

        SwapItems(incoming, fromSlot);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlaceItem(InventoryItem item)
    {
        CurrentItem = item;
        item.GetComponent<DraggableItem>().OriginalSlot = this;

        item.transform.SetParent(transform, false);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        SyncHUDMirror(item);
    }

    public void ClearSlot(bool destroy = false)
    {
        if (CurrentItem == null) return;
        if (destroy) Destroy(CurrentItem.gameObject);
        CurrentItem = null;

        SyncHUDMirror(null);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AcceptItem(InventoryItem item, InventorySlot fromSlot)
    {
        fromSlot.CurrentItem = null;
        CurrentItem = item;
        item.GetComponent<DraggableItem>().OriginalSlot = this;

        item.transform.SetParent(transform, false);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        SyncHUDMirror(item);
        fromSlot.SyncHUDMirror(null);
    }

    private void SwapItems(InventoryItem incoming, InventorySlot fromSlot)
    {
        InventoryItem existing = CurrentItem;

        fromSlot.CurrentItem = existing;
        existing.GetComponent<DraggableItem>().OriginalSlot = fromSlot;
        existing.transform.SetParent(fromSlot.transform, false);
        existing.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        CurrentItem = incoming;
        incoming.GetComponent<DraggableItem>().OriginalSlot = this;
        incoming.transform.SetParent(transform, false);
        incoming.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        SyncHUDMirror(incoming);
        fromSlot.SyncHUDMirror(existing);
    }

    // Находит HUD-зеркало с тем же quickSlotIndex и синхронизирует его содержимое
    internal void SyncHUDMirror(InventoryItem item)
    {
        if (!isQuickSlot) return;

        InventorySlot mirror = InventorySlotManager.Instance.GetHUDMirror(quickSlotIndex);
        if (mirror == null) return;

        // Чистим старый клон
        mirror.CurrentItem = null;
        foreach (Transform child in mirror.transform)
            Destroy(child.gameObject);

        if (item == null) return;

        InventoryItem clone = InventorySlotManager.Instance.CloneVisual(item);

        // Убираем DraggableItem — HUD только отображает, не перетаскивает
        DraggableItem drag = clone.GetComponent<DraggableItem>();
        if (drag != null) Destroy(drag);

        // Вставляем в HUD-слот и подгоняем размер
        RectTransform cloneRect = clone.GetComponent<RectTransform>();

        clone.transform.SetParent(mirror.transform, false);

        cloneRect.anchorMin = new Vector2(0.5f, 0.5f);
        cloneRect.anchorMax = new Vector2(0.5f, 0.5f);
        cloneRect.pivot = new Vector2(0.5f, 0.5f);
        cloneRect.anchoredPosition = Vector2.zero;

        // Масштабируем иконку относительно оригинала через hudIconScale
        Vector2 originalSize = item.GetComponent<RectTransform>().sizeDelta;
        cloneRect.sizeDelta = originalSize * hudIconScale;

        mirror.CurrentItem = clone;
    }
}