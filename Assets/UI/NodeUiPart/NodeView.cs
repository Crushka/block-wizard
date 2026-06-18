
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;

//public class NodeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
//{
//    public NodeModel Data;
//    public TextMeshProUGUI typeText;
//    public TextMeshProUGUI weightText;
//    public Image background;
//    public GameObject connectionPort;

//    public static Vector2 TargetFieldSize = new Vector2(160, 90);
//    private Vector2 _initialSize;
//    private Canvas _canvas;
//    private RectTransform _rt;
//    private CanvasGroup _cg;

//    private bool _isDragClone = false;
//    private bool _removedFromGraph = false;

//    public void Initialize(NodeModel data)
//    {
//        Data = data;
//        _rt = GetComponent<RectTransform>();
//        _canvas = GetComponentInParent<Canvas>();
//        _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
//        _initialSize = _rt.sizeDelta;
//        _removedFromGraph = false;
//        _isDragClone = false;
//        UpdateVisuals();
//    }

//    public void SetVisualState(bool isOnField)
//    {
//        if (_rt == null) _rt = GetComponent<RectTransform>();
//        _rt.localScale = Vector3.one;
//        _rt.sizeDelta = isOnField ? TargetFieldSize : _initialSize;
//        LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
//    }

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        if (eventData.button == PointerEventData.InputButton.Right)
//        {
//            if (NodeEditorManager.Instance != null)
//                NodeEditorManager.Instance.OnNodeRightClick(this);
//        }
//    }

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        if (eventData.button != PointerEventData.InputButton.Left) return;
//        if (Data.type == ElementType.None) { eventData.pointerDrag = null; return; }

//        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

//        bool isInInventory = InventoryManager.Instance != null
//            && transform.IsChildOf(InventoryManager.Instance.inventoryContainer);

//        if (isInInventory)
//        {
//            NodeView clone = InventoryManager.Instance.SpawnDragClone(this, _canvas);
//            clone._isDragClone = true;
//            clone._canvas = _canvas;
//            clone._removedFromGraph = false;

//            CanvasGroup cloneCg = clone.GetComponent<CanvasGroup>()
//                                  ?? clone.gameObject.AddComponent<CanvasGroup>();
//            cloneCg.blocksRaycasts = false;
//            cloneCg.alpha = 0.6f;
//            clone._cg = cloneCg;

//            eventData.pointerDrag = clone.gameObject;
//            return;
//        }
//        if (!_removedFromGraph && NodeEditorManager.Instance != null)
//        {
//            NodeEditorManager.Instance.RemoveNodeFromGraph(this);
//            _removedFromGraph = true;
//        }

//        transform.SetParent(_canvas.transform, true);
//        if (_cg != null) { _cg.blocksRaycasts = false; _cg.alpha = 0.6f; }
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        if (eventData.button != PointerEventData.InputButton.Left) return;

//        if (_rt == null) _rt = GetComponent<RectTransform>();
//        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

//        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;

//        if (NodeEditorManager.Instance != null)
//            NodeEditorManager.Instance.RefreshLines();
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        if (_cg != null) { _cg.blocksRaycasts = true; _cg.alpha = 1f; }

//        if (_canvas != null && transform.parent == _canvas.transform)
//        {
//            if (_isDragClone)
//            {
//                InventoryManager.Instance?.DestroyDragClone(this);
//            }
//            else
//            {
//                Destroy(gameObject);
//            }
//        }
//    }

//    public void UpdateVisuals()
//    {
//        if (Data == null) return;
//        if (typeText != null) typeText.text = Data.type.ToString();
//        if (weightText != null) weightText.text = (Data.weight == int.MaxValue) ? "∞" : Data.weight.ToString();
//        if (background != null) background.color = GetColor(Data.type);
//    }

//    private Color GetColor(ElementType t) => t switch
//    {
//        ElementType.Fire => Color.red,
//        ElementType.Water => Color.blue,
//        ElementType.Earth => new Color(0.4f, 0.3f, 0.1f),
//        ElementType.Air => Color.cyan,
//        ElementType.Cold => new Color(0.5f, 0.8f, 1f),
//        _ => Color.gray
//    };
//}


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    private bool _isDragClone = false;
    private bool _removedFromGraph = false;

    public void Initialize(NodeModel data)
    {
        Data = data;
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _initialSize = _rt.sizeDelta;
        _removedFromGraph = true;
        _isDragClone = false;
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
                NodeEditorManager.Instance.OnNodeRightClick(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (Data.type == ElementType.None) { eventData.pointerDrag = null; return; }

        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        bool isInInventory = InventoryManager.Instance != null
            && transform.IsChildOf(InventoryManager.Instance.inventoryContainer);

        if (isInInventory)
        {
            NodeView clone = InventoryManager.Instance.SpawnDragClone(this, _canvas);
            clone._isDragClone = true;
            clone._canvas = _canvas;
            clone._removedFromGraph = false;

            CanvasGroup cloneCg = clone.GetComponent<CanvasGroup>()
                                  ?? clone.gameObject.AddComponent<CanvasGroup>();
            cloneCg.blocksRaycasts = false;
            cloneCg.alpha = 0.6f;
            clone._cg = cloneCg;

            eventData.pointerDrag = clone.gameObject;
            return;
        }
        //if (!_removedFromGraph && NodeEditorManager.Instance != null)
        //{
        //    NodeEditorManager.Instance.RemoveNodeFromGraph(this);
        //    _removedFromGraph = true;
        //}

        transform.SetParent(_canvas.transform, true);
        if (_cg != null) { _cg.blocksRaycasts = false; _cg.alpha = 0.6f; }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_rt == null) _rt = GetComponent<RectTransform>();
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;

        if (NodeEditorManager.Instance != null)
            NodeEditorManager.Instance.RefreshLines();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_cg != null) { _cg.blocksRaycasts = true; _cg.alpha = 1f; }

        if (_canvas != null && transform.parent == _canvas.transform)
        {
            if (_isDragClone)
            {
                InventoryManager.Instance?.DestroyDragClone(this);
            }
            else
            {
                if (NodeEditorManager.Instance != null)
                {
                    NodeEditorManager.Instance.RemoveNodeFromGraph(this);
                }
                Destroy(gameObject);
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