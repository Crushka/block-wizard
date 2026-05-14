using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class NodeEditorManager : MonoBehaviour, IDropHandler, IScrollHandler
{
    public static NodeEditorManager Instance;
    public GraphModel Graph = new GraphModel();
    
    public RectTransform graphContainer;
    public RectTransform lineContainer;
    public RectTransform plateTransform;
    public GameObject nodePrefab;
    public GameObject connectionPrefab;

    [Header("Zoom Settings")]
    public float minZoom = 0.3f;
    public float maxZoom = 3f;
    public float zoomSensitivity = 0.1f;

    private List<ConnectionView> activeLines = new List<ConnectionView>();

    private NodeView currentSource;
    private ConnectionView tempLine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("<color=green>NodeEditorManager успешно инициализирован!</color>");
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Обнаружен дубликат NodeEditorManager! Удаляю лишний.");
            Destroy(gameObject);
        }
    }

    public void InitEditor() => SpawnStartNode();

    void Update()
    {
        if (graphContainer == null || Mouse.current == null) return;

        if (currentSource != null && tempLine != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, mousePosition, null, out var mousePos);

            Vector2 startScreenPos = RectTransformUtility.WorldToScreenPoint(null, currentSource.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, startScreenPos, null, out var startPos);
            
            tempLine.UpdatePoints(startPos, mousePos);

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                FinishConnection(mousePosition);
            }
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            graphContainer.anchoredPosition += delta;
            if (lineContainer != null) lineContainer.anchoredPosition += delta;
        }
    }

    private void SpawnStartNode()
    {
        if (graphContainer == null) return;

        GameObject startObj = Instantiate(nodePrefab, graphContainer);
        NodeView view = startObj.GetComponent<NodeView>();
        view.Initialize(new NodeModel(ElementType.None)); 
        
        startObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        view.SetVisualState(true); 

        if (!Graph.Nodes.Contains(view.Data)) Graph.Nodes.Add(view.Data);
        RefreshGraph();
    }

    public void RefreshGraph()
    {
        Graph.RecalculateWeights();

        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            if (activeLines[i] != null) Destroy(activeLines[i].gameObject);
        }
        activeLines.Clear();

        var views = graphContainer.GetComponentsInChildren<NodeView>().ToList();
        HashSet<string> drawn = new HashSet<string>();

        foreach (var nv in views)
        {
            nv.UpdateVisuals();
            foreach (var tId in nv.Data.connectedIds)
            {
                string key = string.Compare(nv.Data.id, tId) < 0 ? nv.Data.id + tId : tId + nv.Data.id;
                
                if (!drawn.Contains(key))
                {
                    var targetView = views.FirstOrDefault(v => v.Data.id == tId);
                    if (targetView != null)
                    {
                        var lineObj = Instantiate(connectionPrefab, lineContainer);
                        var line = lineObj.GetComponent<ConnectionView>();
                        line.Initialize(nv, targetView, nv.Data.id, tId);
                        activeLines.Add(line);
                        drawn.Add(key);
                    }
                }
            }
        }
    }

    public void RefreshLines() => activeLines.ForEach(l => { if(l) l.UpdateLine(); });

    public void RemoveNodeFromGraph(NodeView nv)
    {
        if (nv == null || nv.Data == null) return;

        string targetId = nv.Data.id;

        var neighbors = Graph.Nodes.Where(n => n.connectedIds.Contains(targetId)).ToList();

        foreach (var neighbor in neighbors)
        {
            neighbor.connectedIds.Remove(targetId);

            neighbor.weight = 1 + neighbor.connectedIds.Count;
        }

        nv.Data.connectedIds.Clear();
        nv.Data.weight = 2147483647;

        if (Graph.Nodes.Contains(nv.Data))
        {
            Graph.Nodes.Remove(nv.Data);
        }

        RefreshGraph();
        nv.UpdateVisuals();
    }

    private void UpdateNodeWeight(NodeModel node)
    {
        if (node == null) return;

        node.weight = 1 + node.connectedIds.Count;
        
        Debug.Log($"Вес ноды {node.id} пересчитан: {node.weight}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            NodeView nv = eventData.pointerDrag.GetComponent<NodeView>();
            if (nv != null && nv.transform.parent != graphContainer)
            {
                nv.transform.SetParent(graphContainer);
                nv.SetVisualState(true);
                if (!Graph.Nodes.Contains(nv.Data)) Graph.Nodes.Add(nv.Data);
                RefreshGraph();
            }
        }
    }

    public void OnScroll(PointerEventData data)
    {
        if (graphContainer == null) return;

        float scrollDelta = data.scrollDelta.y * zoomSensitivity;
        Vector3 newScale = graphContainer.localScale + new Vector3(scrollDelta, scrollDelta, 0);

        newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
        newScale.z = 1f;

        graphContainer.localScale = newScale;

        RefreshLines();
    }

    public void OnNodeRightClick(NodeView source)
    {
        currentSource = source;
        GameObject lineObj = Instantiate(connectionPrefab, lineContainer);
        tempLine = lineObj.GetComponent<ConnectionView>();
        tempLine.transform.SetAsFirstSibling();
    }

    private string GetRootBranchId(NodeView node)
    {
        if (node.Data.type == ElementType.None) return "START";

        HashSet<string> visited = new HashSet<string>();
        List<string> toCheck = new List<string> { node.Data.id };

        while (toCheck.Count > 0)
        {
            string currentId = toCheck[0];
            toCheck.RemoveAt(0);
            visited.Add(currentId);

            NodeModel currentModel = Graph.Nodes.Find(n => n.id == currentId);
            
            foreach (string neighborId in currentModel.connectedIds)
            {
                NodeModel neighbor = Graph.Nodes.Find(n => n.id == neighborId);

                if (neighbor.type == ElementType.None) return currentId;

                if (!visited.Contains(neighborId))
                {
                    toCheck.Add(neighborId);
                }
            }
        }
        return null;
    }

    private void FinishConnection(Vector2 screenMousePos)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenMousePos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        NodeView targetNode = null;

        foreach (var result in results)
        {
            if (result.gameObject.transform.IsChildOf(lineContainer)) continue;
            targetNode = result.gameObject.GetComponentInParent<NodeView>();

            if (targetNode != null && targetNode != currentSource) break; 
        }

        if (targetNode != null)
        {
            string rootSource = GetRootBranchId(currentSource);
            string rootTarget = GetRootBranchId(targetNode);

            bool isDifferentBranches = rootSource != null && rootTarget != null && 
                                       rootSource != "START_NODE" && rootTarget != "START_NODE" && 
                                       rootSource != rootTarget;

            if (isDifferentBranches)
            {
                Debug.LogError("[Logic Error] Нельзя соединять разные подграфы!");
                return;
            }
            else if (!currentSource.Data.connectedIds.Contains(targetNode.Data.id))
            {
                currentSource.Data.AddLink(targetNode.Data.id);
                targetNode.Data.AddLink(currentSource.Data.id);
                RefreshGraph(); 
            }
        }

        if (tempLine != null) Destroy(tempLine.gameObject);
        currentSource = null;
        tempLine = null;
    }
}