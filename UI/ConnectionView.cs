using UnityEngine;
using UnityEngine.UI;

public class ConnectionView : MonoBehaviour
{
    public NodeView sourceView;
    public NodeView targetView;
    public string idA; 
    public string idB; 
    
    public float thickness = 15f; 
    private RectTransform _rt;
    private Image _image;

    public void Initialize(NodeView start, NodeView end, string idA, string idB)
    {
        this.sourceView = start; this.targetView = end;
        this.idA = idA; this.idB = idB;
        _rt = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        
        if (_image != null) _image.raycastTarget = true;
        
        _rt.pivot = new Vector2(0, 0.5f);
        _rt.anchorMin = new Vector2(0.5f, 0.5f);
        _rt.anchorMax = new Vector2(0.5f, 0.5f);
        UpdateLine();
    }

    public void UpdateLine()
    {
        if (sourceView == null || targetView == null) return;

        Vector2 screenStart = RectTransformUtility.WorldToScreenPoint(null, sourceView.transform.position);
        Vector2 screenEnd = RectTransformUtility.WorldToScreenPoint(null, targetView.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt.parent as RectTransform, screenStart, null, out var localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt.parent as RectTransform, screenEnd, null, out var localEnd);

        UpdatePoints(localStart, localEnd);
    }

    public void UpdatePoints(Vector2 startL, Vector2 endL)
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        Vector2 direction = endL - startL;
        _rt.anchoredPosition = startL;
        _rt.sizeDelta = new Vector2(direction.magnitude, thickness);
        _rt.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }
}