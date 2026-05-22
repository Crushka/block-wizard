using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class NodeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public NodeModel Data;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI weightText;
    public Image background;
    public GameObject connectionPort;

    public static Vector2 TargetFieldSize = new Vector2(160, 90);
    private Vector2 _initialSize;
    private Canvas _canvas;
    private RectTransform _rt;
    private CanvasGroup _cg;

    public void Initialize(NodeModel data)
    {
        Data = data;
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>(true);
        _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _initialSize = _rt.sizeDelta;

        Debug.Log($"[NodeView.Init] {data.type} | canvas={(_canvas != null ? _canvas.name : "NULL")} | scaleFactor={(_canvas != null ? _canvas.scaleFactor : 0)}");

        UpdateVisuals();
    }

    public void SetVisualState(bool isOnField)
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();
        _rt.localScale = Vector3.one;
        _rt.sizeDelta = isOnField ? TargetFieldSize : _initialSize;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (NodeEditorManager.Instance != null)
            {
                NodeEditorManager.Instance.OnNodeRightClick(this);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (Data.type == ElementType.None) { eventData.pointerDrag = null; return; }

        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        Debug.Log($"[NodeView.BeginDrag] canvas={(_canvas != null ? _canvas.name : "NULL")} | parent before={transform.parent?.name}");
        transform.SetParent(_canvas.transform, true);

        Debug.Log($"[NodeView.BeginDrag] parent after={transform.parent?.name}");
        if (_cg != null) { _cg.blocksRaycasts = false; _cg.alpha = 0.6f; }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_rt == null) _rt = GetComponent<RectTransform>();
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        
        if (NodeEditorManager.Instance != null)
        {
            NodeEditorManager.Instance.RefreshLines();
        }
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
            var manager = NodeEditorManager.Instance;
            
            if (manager != null)
            {
                manager.RemoveNodeFromGraph(this);

                InventoryManager.Instance.MoveToInventory(this);
            }
        }
    }

    public void UpdateVisuals()
    {
        if (Data == null) return;
        if (typeText != null) typeText.text = Data.type.ToString();
        if (weightText != null) weightText.text = (Data.weight == int.MaxValue) ? "∞" : Data.weight.ToString();
        if (background != null) background.color = GetColor(Data.type);
    }

    private Color GetColor(ElementType t) => t switch
    {
        ElementType.Fire => Color.red,
        ElementType.Water => Color.blue,
        ElementType.Earth => new Color(0.4f, 0.3f, 0.1f),
        ElementType.Air => Color.cyan,
        ElementType.Cold => new Color(0.5f, 0.8f, 1f),
        _ => Color.gray
    };
}