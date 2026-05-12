using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConnectionView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public NodeView sourceView; 
    public NodeView targetView; 
    public RectTransform rectTransform; 
    public Image lineImage; 
    public float lineWidth = 5f; 

    [HideInInspector] public string idA; 
    [HideInInspector] public string idB; 

    private Image _img;
    private Color _origColor;
    public Color hoverColor = Color.red;

    private void Awake()
    {
        _img = lineImage != null ? lineImage : GetComponent<Image>();
        if (_img != null) _origColor = _img.color;
    }

    public void Initialize(NodeView start, NodeView end, string idA, string idB) {
        this.sourceView = start; this.targetView = end;
        this.idA = idA; this.idB = idB;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0, 0.5f);
        UpdateLine();
    }

    public void UpdateLine() {
        if (sourceView == null || targetView == null) return;
        Vector2 start = sourceView.transform.position;
        Vector2 end = targetView.transform.position;

        RectTransform parentRect = rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, RectTransformUtility.WorldToScreenPoint(null, start), null, out var localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, RectTransformUtility.WorldToScreenPoint(null, end), null, out var localEnd);
        UpdatePoints(localStart, localEnd);
    }

    public void UpdatePoints(Vector2 startL, Vector2 endL) {
        Vector2 dir = endL - startL;
        rectTransform.anchoredPosition = startL;
        rectTransform.sizeDelta = new Vector2(dir.magnitude, lineWidth);
        rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_img != null) _img.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_img != null) _img.color = _origColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (sourceView != null && targetView != null)
            {
                sourceView.Data.connectedIds.Remove(idB);
                targetView.Data.connectedIds.Remove(idA);

                if (NodeEditorManager.Instance != null)
                    NodeEditorManager.Instance.RefreshGraph();
            }
        }
    }
}