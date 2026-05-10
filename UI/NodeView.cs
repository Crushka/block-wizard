using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class NodeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public NodeModel Data;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Image background;
    public GameObject connectionPort; 

    public static Vector2 TargetFieldSize = new Vector2(160, 90);
    private Vector2 _initialSize; 
    private Canvas _canvas;
    private RectTransform _rt;
    private CanvasGroup _cg;
    [HideInInspector] public Transform lastStableParent; 

    public void Initialize(NodeModel data)
    {
        Data = data;
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _initialSize = _rt.sizeDelta;
        
        // Принудительно ставим масштаб 1 при инициализации
        _rt.localScale = Vector3.one; 
        
        UpdateVisuals();
    }

    public void SetVisualState(bool isOnField)
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();

        _rt.localScale = Vector3.one; 
        
        _rt.sizeDelta = isOnField ? TargetFieldSize : _initialSize;
        
        // Принудительно обновляем UI, чтобы размер применился мгновенно
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (connectionPort != null && RectTransformUtility.RectangleContainsScreenPoint(connectionPort.GetComponent<RectTransform>(), eventData.position, null))
        {
            NodeEditorManager.Instance.StartConnection(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        bool isPort = connectionPort != null && RectTransformUtility.RectangleContainsScreenPoint(connectionPort.GetComponent<RectTransform>(), eventData.position, null);
        if (Data.type == MagicType.Magic || isPort) { eventData.pointerDrag = null; return; }

        lastStableParent = transform.parent;

        transform.SetParent(_canvas.transform, false);

        _rt.localScale = Vector3.one;

        if (_cg != null) { _cg.blocksRaycasts = false; _cg.alpha = 0.6f; }
    }

    public void OnDrag(PointerEventData eventData) 
    {
        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        NodeEditorManager.Instance.RefreshLines();
    }

    public void OnEndDrag(PointerEventData eventData) 
    {
        if (_cg != null) { _cg.blocksRaycasts = true; _cg.alpha = 1f; }
        StartCoroutine(CheckDrop());
    }

    private IEnumerator CheckDrop()
    {
        yield return new WaitForEndOfFrame();
        if (transform.parent == _canvas.transform) 
        {
            InventoryManager.Instance.MoveToInventory(this);
            _rt.localScale = Vector3.one;
        }
    }

    public void UpdateVisuals()
    {
        if (Data == null) return;
        if (typeText != null) typeText.text = Data.type.ToString();
        
        if (weightText != null)
            weightText.text = (Data.weight == int.MaxValue) ? "∞" : Data.weight.ToString();
            
        if (background != null) background.color = GetColor(Data.type);
    }

    private Color GetColor(MagicType t) => t switch {
        MagicType.Fire => Color.red, 
        MagicType.Water => Color.blue,
        MagicType.Earth => new Color(0.4f, 0.2f, 0.1f), 
        MagicType.Air => Color.cyan,
        MagicType.Magic => Color.magenta, 
        _ => Color.white
    };
}