// SpellDragItem.cs
// Компонент висит на AbilityBack (или на resultSlotImage) постоянно.
// При начале drag создаёт временный ghost-объект, который летает под курсором.
// Сам AbilityBack никуда не перемещается и не уничтожается.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SpellDragItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // -1  → слот результата
    //  0+ → индекс HUD-слота
    public int SourceSlotIndex { get; set; } = -1;
    public NodeBase SpellNode { get; set; }

    // ── private ───────────────────────────────────────────────────────────────
    private Canvas _rootCanvas;
    private GameObject _ghost;          // временный объект под курсором
    private RectTransform _ghostRt;

    // ── IBeginDragHandler ────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SpellNode == null) { eventData.pointerDrag = null; return; }

        // Находим root canvas один раз
        if (_rootCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) _rootCanvas = c.rootCanvas;
        }
        if (_rootCanvas == null) return;

        // Создаём ghost: простой полупрозрачный Image с текстом
        _ghost = new GameObject("SpellDragGhost");
        _ghostRt = _ghost.AddComponent<RectTransform>();
        _ghost.transform.SetParent(_rootCanvas.transform, false);
        _ghost.transform.SetAsLastSibling();

        // Размер ghost = размер исходного слота
        var srcRt = GetComponent<RectTransform>();
        _ghostRt.sizeDelta = srcRt != null ? srcRt.rect.size : new Vector2(60f, 60f);

        // Цвет — тинт заклинания
        var img = _ghost.AddComponent<Image>();
        img.color = new Color(
            SpellNode.PrimaryColor.r,
            SpellNode.PrimaryColor.g,
            SpellNode.PrimaryColor.b, 0.7f);
        img.raycastTarget = false; // ghost не мешает raycast на drop-zones

        // Текст
        var labelGO = new GameObject("GhostLabel");
        labelGO.transform.SetParent(_ghost.transform, false);
        var labelRt = labelGO.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = SpellIconHelper.GetIconText(SpellNode);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 11f;
        tmp.raycastTarget = false;

        // Ставим ghost под курсор
        MoveGhostToPointer(eventData);
    }

    // ── IDragHandler ─────────────────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost == null) return;
        MoveGhostToPointer(eventData);
    }

    // ── IEndDragHandler ──────────────────────────────────────────────────────
    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();
        // AbilityBack остаётся на месте — ничего возвращать не нужно
    }

    // Вызывается из SpellSlotDropZone когда drop принят
    public void ConsumeByDrop()
    {
        DestroyGhost();
        // Компонент SpellDragItem НЕ уничтожается — он останется на AbilityBack.
        // SpellSlotUI сам обновит его данные через RefreshAll после SwapSlots/AssignToSlot.
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    private void MoveGhostToPointer(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_rootCanvas.transform,
            eventData.position,
            _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera,
            out Vector2 localPoint);

        _ghostRt.anchoredPosition = localPoint;
    }

    private void DestroyGhost()
    {
        if (_ghost != null) { Destroy(_ghost); _ghost = null; _ghostRt = null; }
    }

    void OnDestroy() => DestroyGhost();
}