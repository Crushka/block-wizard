using System.Collections.Generic;
using UnityEngine;

public class SpellSlot
{
    public int index;
    public GraphModel graph = null; 

    [System.NonSerialized]
    public NodeBase compiledNode = null; 

    public bool IsEmpty => graph == null || graph.Nodes.Count == 0;

    public SpellSlot(int idx) { index = idx; }

    public void SnapshotFromEditor(NodeEditorManager mgr)
    {
        if (mgr?.Graph == null) { graph = null; return; }

        graph = new GraphModel();
        foreach (var src in mgr.Graph.Nodes)
        {
            var copy = new NodeModel(src.type) { id = src.id, weight = src.weight };
            foreach (var id in src.connectedIds) copy.AddLink(id);
            graph.Nodes.Add(copy);
        }
        Debug.Log($"[SpellSlot {index}] снимок: {graph.Nodes.Count} узлов");
    }

    public void RestoreToEditor(NodeEditorManager mgr)
    {
        if (mgr == null) return;

        mgr.InitEditor();

        if (graph == null || graph.Nodes.Count == 0)
        {
            Debug.Log($"[SpellSlot {index}] пустой слот → редактор сброшен");
            return;
        }

        var savedNone = graph.Nodes.Find(n => n.type == ElementType.None);
        if (savedNone != null)
        {
            var liveNone = mgr.Graph.Nodes.Find(n => n.type == ElementType.None);
            if (liveNone != null) liveNone.id = savedNone.id;
        }

        foreach (var node in graph.Nodes)
        {
            if (node.type == ElementType.None) continue;
            mgr.Graph.Nodes.Add(node);
            var obj = Object.Instantiate(mgr.nodePrefab, mgr.graphContainer);
            var view = obj.GetComponent<NodeView>();
            view.Initialize(node);
            view.SetVisualState(true);
        }

        mgr.RefreshGraph();
        Debug.Log($"[SpellSlot {index}] восстановлен в редактор: {graph.Nodes.Count} узлов");
    }

    public NodeBase Compile()
    {
        if (graph == null || graph.Nodes.Count == 0)
        {
            compiledNode = null;
            return null;
        }

        try
        {
            var calcGraph = new SpellGraph();
            var map = new System.Collections.Generic.Dictionary<string, GraphNode>();

            foreach (var uiNode in graph.Nodes)
            {
                NodeBase data = graph.GetElementData(uiNode.type);
                if (data == null) continue;

                int num = (uiNode.weight == int.MaxValue) ? 0 : uiNode.weight;
                float w = data.Weight > 0 ? data.Weight : 1.0f;

                var gn = calcGraph.CreateNode(data, num, w);
                map[uiNode.id] = gn;

                if (uiNode.type == ElementType.None)
                    calcGraph.StartNode = gn;
            }

            var processed = new System.Collections.Generic.HashSet<string>();
            foreach (var uiNode in graph.Nodes)
            {
                foreach (var targetId in uiNode.connectedIds)
                {
                    string key = string.Compare(uiNode.id, targetId) < 0
                        ? $"{uiNode.id}_{targetId}"
                        : $"{targetId}_{uiNode.id}";
                    if (!processed.Contains(key) && map.ContainsKey(uiNode.id) && map.ContainsKey(targetId))
                    {
                        calcGraph.Connect(map[uiNode.id], map[targetId]);
                        processed.Add(key);
                    }
                }
            }

            if (calcGraph.StartNode == null || calcGraph.Nodes.Count < 2)
            {
                compiledNode = null;
                return null;
            }

            calcGraph.CalculateWeight();
            compiledNode = SpellGraphSolver.SlowGraph(calcGraph);
            Debug.Log($"[SpellSlot {index}] скомпилирован: {compiledNode?.GetDominantAttack()} dmg={compiledNode?.Damage:F1}");
            return compiledNode;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SpellSlot {index}] ошибка компиляции: {ex.Message}");
            compiledNode = null;
            return null;
        }
    }
}