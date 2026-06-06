using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to the InventoryItemPrefab root alongside InventoryItem.
/// Handles the drag visual: lifts the icon to canvas root, follows pointer,
/// snaps back if dropped on invalid target.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventorySlot OriginalSlot { get; set; }

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        // НЕ ищем Canvas здесь — префаб ещё не вложен в Canvas в момент Awake
    }

    // Ищем Canvas лениво — к моменту первого drag он уже точно есть в иерархии
    private Canvas RootCanvas
    {
        get
        {
            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            return _rootCanvas;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (RootCanvas == null)
        {
            Debug.LogError("[DraggableItem] Canvas не найден. Убедись, что prefab находится внутри Canvas.");
            return;
        }

        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        // Поднимаем на корень Canvas — рендерится поверх всего
        transform.SetParent(RootCanvas.transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.75f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RootCanvas == null) return;
        _rectTransform.anchoredPosition += eventData.delta / RootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        // Если OnDrop не был вызван (промах мимо слота) — возвращаем обратно
        if (RootCanvas != null && transform.parent == RootCanvas.transform)
            ReturnToOriginalSlot();
    }

    public void ReturnToOriginalSlot()
    {
        transform.SetParent(_originalParent, false);
        transform.SetSiblingIndex(_originalSiblingIndex);
        _rectTransform.anchoredPosition = Vector2.zero;
    }
}