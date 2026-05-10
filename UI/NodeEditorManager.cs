using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeEditorManager : MonoBehaviour
{
    [Header("UI ссылки")]
    public RectTransform plateTransform;
    public GameObject nodePrefab;
    public Transform lineContainer;

    [Header("Боевые ссылки")]
    public AttackFactory attackFactory;
    public Transform firePoint;

    [Header("Данные")]
    public GraphModel Graph = new GraphModel();
    private List<NodeView> _nodeViews = new List<NodeView>();
    private GraphIntegrator _integrator;
    private IAttack _preparedAttack;

    void Awake()
    {
        _integrator = GetComponent<GraphIntegrator>() ?? gameObject.AddComponent<GraphIntegrator>();
    }

    public void AddNodeAtPosition(int typeIndex, Vector2 screenPos)
    {
        MagicType type = (MagicType)typeIndex;
        GameObject go = Instantiate(nodePrefab, plateTransform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(plateTransform, screenPos, null, out Vector2 localPos);
        go.GetComponent<RectTransform>().anchoredPosition = localPos;

        NodeModel model = new NodeModel(type);
        NodeView view = go.GetComponent<NodeView>();
        
        view.Setup(model, this);
        Graph.Nodes.Add(model);
        _nodeViews.Add(view);

        RefreshGraph();
    }

    public void RefreshGraph()
    {
        Graph.RecalculateWeights(); // Твой BFS
        foreach (var view in _nodeViews)
        {
            view.UpdateUI();
        }

        PrepareSpellSilent();
    }

    private void PrepareSpellSilent()
    {
        if (Graph.Nodes.Count == 0) return;
        SpellGraph backend = _integrator.ConvertToBackendGraph(Graph.Nodes);
        if (backend.StartNode == null) return;

        backend.CalculateWeight();
        NodeBase result = SpellGraphSolver.SlowGraph(backend);
        
        if (attackFactory != null)
            _preparedAttack = attackFactory.GetAttack(result);
    }

    public void Fire() => _preparedAttack?.Cast(firePoint);

    public bool TryConnect(NodeView a, NodeView b)
    {
        if (a == b) return false;
        
        if (!CanConnect(a.Model, b.Model)) return false;

        a.Model.AddLink(b.Model.id);
        b.Model.AddLink(a.Model.id);
        
        RefreshGraph();
        return true;
    }

    private bool CanConnect(NodeModel a, NodeModel b) 
    {
        if (a.type == MagicType.Magic || b.type == MagicType.Magic) return true;
        return AreInSameSubGraph(a, b);
    }

    private bool AreInSameSubGraph(NodeModel start, NodeModel target) 
    {
        HashSet<string> visited = new HashSet<string>();
        Queue<NodeModel> queue = new Queue<NodeModel>();
        queue.Enqueue(start);
        visited.Add(start.id);

        while (queue.Count > 0) {
            var curr = queue.Dequeue();
            if (curr.id == target.id) return true;
            foreach (var id in curr.connectedIds) {
                var neighbor = Graph.Nodes.Find(n => n.id == id);
                if (neighbor == null || neighbor.type == MagicType.Magic) continue;
                if (visited.Add(neighbor.id)) queue.Enqueue(neighbor);
            }
        }
        return false;
    }
}