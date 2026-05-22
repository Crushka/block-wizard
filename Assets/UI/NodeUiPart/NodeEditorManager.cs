
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

    private NodeView _connectionSource;
    private ConnectionView _tempLine;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public void InitEditor() => SpawnStartNode();

    void Update()
    {
        if (graphContainer == null || Mouse.current == null) return;

        if (_connectionSource != null && _tempLine != null)
        {
            if (!_connectionSource)
            {
                CancelTempLine();
                return;
            }

            Vector2 mouseScreen = Mouse.current.position.ReadValue();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                lineContainer, mouseScreen, null, out var localMouse);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                lineContainer,
                RectTransformUtility.WorldToScreenPoint(null, _connectionSource.transform.position),
                null, out var localSrc);

            _tempLine.UpdatePoints(localSrc, localMouse);

            if (Mouse.current.rightButton.wasReleasedThisFrame)
                FinishConnection(mouseScreen);
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 d = Mouse.current.delta.ReadValue();
            graphContainer.anchoredPosition += d;
            if (lineContainer != null) lineContainer.anchoredPosition += d;
        }
    }


    private void SpawnStartNode()
    {
        if (graphContainer == null) return;

        var obj = Instantiate(nodePrefab, graphContainer);
        var view = obj.GetComponent<NodeView>();
        view.Initialize(new NodeModel(ElementType.None));
        obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        view.SetVisualState(true);

        if (!Graph.Nodes.Contains(view.Data))
            Graph.Nodes.Add(view.Data);

        RefreshGraph();
    }


    public void RefreshGraph()
    {
        Graph.RecalculateWeights();
        RebuildLines();
    }

    private void RebuildLines()
    {
        GameObject tempGO = _tempLine != null ? _tempLine.gameObject : null;

        for (int i = lineContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = lineContainer.GetChild(i).gameObject;
            if (child == tempGO) continue;
            DestroyImmediate(child);
        }
        activeLines.Clear();

        var views = graphContainer.GetComponentsInChildren<NodeView>().ToList();
        var drawn = new HashSet<string>();

        foreach (var nv in views)
        {
            nv.UpdateVisuals();
            foreach (var tId in nv.Data.connectedIds)
            {
                string key = EdgeKey(nv.Data.id, tId);
                if (drawn.Contains(key)) continue;

                var target = views.FirstOrDefault(v => v.Data.id == tId);
                if (target == null) continue;

                var lineObj = Instantiate(connectionPrefab, lineContainer);
                var conn = lineObj.GetComponent<ConnectionView>();
                conn.Initialize(nv, target, nv.Data.id, tId);
                activeLines.Add(conn);
                drawn.Add(key);
            }
        }
    }

    public void RefreshLines()
    {
        foreach (var l in activeLines)
            if (l != null) l.UpdateLine();
    }

    public void RemoveEdge(string idA, string idB)
    {
        var nodeA = Graph.Nodes.Find(n => n.id == idA);
        var nodeB = Graph.Nodes.Find(n => n.id == idB);

        if (nodeA != null) nodeA.connectedIds.Remove(idB);
        if (nodeB != null) nodeB.connectedIds.Remove(idA);

        RefreshGraph();
    }

    public void RemoveNodeFromGraph(NodeView nv)
    {
        if (nv == null || nv.Data == null) return;

        CancelTempLine();

        string removedId = nv.Data.id;

        foreach (var node in Graph.Nodes)
            node.connectedIds.Remove(removedId);

        nv.Data.connectedIds.Clear();
        Graph.Nodes.Remove(nv.Data);
        Graph.RecalculateWeights();
        RebuildLines();
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var nv = eventData.pointerDrag.GetComponent<NodeView>();
        if (nv == null) return;

        if (!Graph.Nodes.Contains(nv.Data))
            Graph.Nodes.Add(nv.Data);

        nv.transform.SetParent(graphContainer, true);
        nv.SetVisualState(true);

        RefreshGraph();
    }

    public void OnScroll(PointerEventData data)
    {
        if (graphContainer == null) return;

        float delta = data.scrollDelta.y * zoomSensitivity;
        var scale = graphContainer.localScale + new Vector3(delta, delta, 0);
        scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
        scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
        scale.z = 1f;
        graphContainer.localScale = scale;

        RefreshLines();
    }

    public void OnNodeRightClick(NodeView source)
    {
        CancelTempLine();

        _connectionSource = source;
        var obj = Instantiate(connectionPrefab, lineContainer);
        _tempLine = obj.GetComponent<ConnectionView>();
        _tempLine.transform.SetAsFirstSibling();
    }

    private void CancelTempLine()
    {
        if (_tempLine != null)
        {
            Destroy(_tempLine.gameObject);
            _tempLine = null;
        }
        _connectionSource = null;
    }

    private void FinishConnection(Vector2 screenPos)
    {
        NodeView source = _connectionSource;
        CancelTempLine();

        if (source == null || !source) return;

        var evData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(evData, results);

        NodeView target = null;
        foreach (var r in results)
        {
            if (r.gameObject.transform.IsChildOf(lineContainer)) continue;
            var candidate = r.gameObject.GetComponentInParent<NodeView>();
            if (candidate != null && candidate != source)
            {
                target = candidate;
                break;
            }
        }

        if (target == null || target.Data == null) return;

        if (source.Data.connectedIds.Contains(target.Data.id)) return;

        string noneId = GetNoneNodeId();

        if (noneId != null)
        {
            string branchSource = GetDirectBranchOfNone(noneId, source.Data.id);
            string branchTarget = GetDirectBranchOfNone(noneId, target.Data.id);

            if (branchSource != null && branchTarget != null && branchSource != branchTarget)
            {
                Debug.LogWarning("[Logic] Нельзя соединять узлы из разных веток START NODE.");
                return;
            }
        }

        string rootSource = GetConnectedStartId(source.Data);
        string rootTarget = GetConnectedStartId(target.Data);

        if (rootSource != null && rootTarget != null && rootSource != rootTarget)
        {
            Debug.LogWarning("[Logic] Нельзя соединять ноды из разных подграфов с None.");
            return;
        }

        source.Data.AddLink(target.Data.id);
        target.Data.AddLink(source.Data.id);
        RefreshGraph();
    }

    public void PruneIsolatedSubgraphs()
    {
        var reachable = new HashSet<string>();

        foreach (var node in Graph.Nodes)
        {
            if (node.type != ElementType.None) continue;

            var queue = new Queue<string>();
            queue.Enqueue(node.id);

            while (queue.Count > 0)
            {
                string id = queue.Dequeue();
                if (reachable.Contains(id)) continue;
                reachable.Add(id);

                var current = Graph.Nodes.Find(n => n.id == id);
                if (current == null) continue;

                foreach (var neighborId in current.connectedIds)
                    if (!reachable.Contains(neighborId))
                        queue.Enqueue(neighborId);
            }
        }

        var toRemove = Graph.Nodes.Where(n => !reachable.Contains(n.id)).ToList();
        if (toRemove.Count == 0) return;

        foreach (var node in Graph.Nodes)
            node.connectedIds.RemoveAll(id => toRemove.Any(r => r.id == id));

        foreach (var node in toRemove)
            Graph.Nodes.Remove(node);

        var views = graphContainer.GetComponentsInChildren<NodeView>().ToList();
        foreach (var view in views)
        {
            if (view.Data == null) continue;
            if (!reachable.Contains(view.Data.id))
                Destroy(view.gameObject);
        }

        RefreshGraph();
    }

    private string GetConnectedStartId(NodeModel startSearch)
    {
        if (startSearch == null) return null;
        if (startSearch.type == ElementType.None) return startSearch.id;

        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(startSearch.id);

        while (queue.Count > 0)
        {
            string id = queue.Dequeue();
            if (visited.Contains(id)) continue;
            visited.Add(id);

            NodeModel node = Graph.Nodes.Find(n => n.id == id);
            if (node == null) continue;
            if (node.type == ElementType.None) return node.id;

            foreach (var neighborId in node.connectedIds)
                if (!visited.Contains(neighborId))
                    queue.Enqueue(neighborId);
        }

        return null;
    }

    private string GetNoneNodeId()
    {
        var none = Graph.Nodes.Find(n => n.type == ElementType.None);
        return none?.id;
    }

    private string GetDirectBranchOfNone(string noneId, string targetId)
    {
        if (targetId == noneId) return null;
        NodeModel noneNode = Graph.Nodes.Find(n => n.id == noneId);
        if (noneNode == null) return null;

        var branchLabel = new Dictionary<string, string>();
        var queue = new Queue<string>();

        foreach (var neighborId in noneNode.connectedIds)
        {
            if (branchLabel.ContainsKey(neighborId)) continue;
            branchLabel[neighborId] = neighborId;
            queue.Enqueue(neighborId);
        }

        while (queue.Count > 0)
        {
            string id = queue.Dequeue();
            NodeModel node = Graph.Nodes.Find(n => n.id == id);
            if (node == null) continue;

            foreach (var neighborId in node.connectedIds)
            {
                if (neighborId == noneId) continue;
                if (branchLabel.ContainsKey(neighborId)) continue;
                branchLabel[neighborId] = branchLabel[id];
                queue.Enqueue(neighborId);
            }
        }

        branchLabel.TryGetValue(targetId, out string branch);
        return branch;
    }

    public void ClearEditor()
    {
        if (graphContainer != null)
        {
            foreach (Transform child in graphContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (lineContainer != null)
        {
            foreach (Transform line in lineContainer)
            {
                Destroy(line.gameObject);
            }
        }
        activeLines.Clear();

        if (Graph != null && Graph.Nodes != null)
        {
            Graph.Nodes.Clear();
        }
    }
    private static string EdgeKey(string a, string b) =>
        string.Compare(a, b, System.StringComparison.Ordinal) < 0
            ? a + "_" + b
            : b + "_" + a;
}