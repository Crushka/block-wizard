using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
// Типы шагов визуализации
// ──────────────────────────────────────────────────────────────────────────────

public enum VizStepType
{
    SubgraphSplit,   // Шаг 2: BFS нашёл узлы подграфа
    CycleFound,      // Шаг 3: найден цикл
    CycleCollapsed,  // Шаг 5: цикл схлопнут в один узел
    TreeCollapsed,   // Шаг 6: дерево свёрнуто
    FinalResult,     // Шаг 8: финальный результат
}

// ──────────────────────────────────────────────────────────────────────────────
// Шаг визуализации.
//
// Nodes — полный снимок графа на этот момент в виде List<NodeModel>.
// NodeModel.id совпадает со стабильным id узла на всё время Solve,
// поэтому NodeView.Initialize(model) / UpdateVisuals() работают напрямую.
//
// HighlightIds   — подмножество NodeModel.id узлов которые надо выделить.
// HighlightEdges — пары (idA, idB) рёбер которые надо выделить.
// ──────────────────────────────────────────────────────────────────────────────

public class VizStep
{
    public VizStepType Type;
    public string Description;
    public List<NodeModel> Nodes;
    public List<string> HighlightIds;
    public List<(string, string)> HighlightEdges;
}

// ──────────────────────────────────────────────────────────────────────────────
// Главный класс — полная замена SpellGraphSolver.SlowGraph()
// ──────────────────────────────────────────────────────────────────────────────

public static class SpellSolverDebug
{
    private static readonly System.Random Rng = new System.Random();
    private const int MaxCollapseIterations = 250;

    // Стабильный string-id для каждого GraphNode в рамках одного вызова Solve.
    private static Dictionary<GraphNode, string> _nodeIds;
    private static int _nextId;

    private static string GetId(GraphNode node)
    {
        if (!_nodeIds.TryGetValue(node, out string id))
        {
            id = (_nextId++).ToString();
            _nodeIds[node] = id;
        }
        return id;
    }

    // ── Публичный метод ────────────────────────────────────────────────────────

    /// <summary>
    /// Полная замена SpellGraphSolver.SlowGraph().
    /// Возвращает (финальный NodeBase, список шагов).
    /// Каждый шаг содержит List&lt;NodeModel&gt; готовый для NodeView.Initialize().
    /// </summary>
    public static (NodeBase result, List<VizStep> steps) Solve(SpellGraph graph)
    {
        _nodeIds = new Dictionary<GraphNode, string>();
        _nextId = 0;

        var steps = new List<VizStep>();

        graph.PrepareGraph();

        // Раздаём id заранее, чтобы они не прыгали при добавлении новых узлов
        foreach (var node in CollectAllNodes(graph))
            GetId(node);

        // ── Шаг 2: BFS-разбивка ───────────────────────────────────────────────
        var separateGraphs = SeparateGraph(graph);

        foreach (var subgraph in separateGraphs)
        {
            steps.Add(new VizStep
            {
                Type = VizStepType.SubgraphSplit,
                Description = $"BFS: найдено {subgraph.Count} узлов в подграфе",
                Nodes = Snapshot(subgraph),
                HighlightIds = subgraph.Select(GetId).ToList(),
                HighlightEdges = EdgesOf(subgraph),
            });

            // ── Шаги 3 + 5: циклы ─────────────────────────────────────────────
            CollapseCyclesWithSteps(subgraph, steps);

            // ── Шаг 6: свёртка дерева ─────────────────────────────────────────
            CollapseTreeWithStep(subgraph, steps);
        }

        // Финальный merge нескольких подграфов
        var collapsedNodes = separateGraphs
            .Where(sg => sg.Count > 0)
            .Select(sg => sg[0])
            .ToList();

        GraphNode finalNode = collapsedNodes[0];
        for (int i = 1; i < collapsedNodes.Count; i++)
            finalNode.Data = MixAlgorithms.MixNodes(finalNode.Data, collapsedNodes[i].Data);

        MixAlgorithms.IncreaseValue(finalNode.Data, null, graph.Weight);
        MixAlgorithms.IncreaseValue(finalNode.Data, null, finalNode.Data.Weight);

        // ── Шаг 8: результат ──────────────────────────────────────────────────
        steps.Add(new VizStep
        {
            Type = VizStepType.FinalResult,
            Description = $"{finalNode.Data.GetDominantAttack()} | dmg={finalNode.Data.Damage:F1} | range={finalNode.Data.Range:F1} | speed={finalNode.Data.Speed:F1}",
            Nodes = Snapshot(new List<GraphNode> { finalNode }),
            HighlightIds = new List<string> { GetId(finalNode) },
            HighlightEdges = new List<(string, string)>(),
        });

        return (finalNode.Data, steps);
    }

    // ── Шаги 3 + 5 ────────────────────────────────────────────────────────────

    private static void CollapseCyclesWithSteps(List<GraphNode> graphList, List<VizStep> steps)
    {
        var graph = new LinkedList<GraphNode>(graphList);
        var nodeSet = new HashSet<GraphNode>(graphList);
        var nodeIndex = new Dictionary<GraphNode, int>(graphList.Count);
        for (int i = 0; i < graphList.Count; i++)
            nodeIndex[graphList[i]] = i;

        var sortedNodes = graphList
            .OrderByDescending(n => n.num)
            .ThenBy(_ => Rng.Next())
            .ToList();

        int iterations = 0;
        while (iterations++ < MaxCollapseIterations)
        {
            var allCycles = FindAllCycles(nodeSet, nodeIndex, sortedNodes);
            if (allCycles.Count == 0) break;

            int minSize = allCycles.Min(c => c.Count);
            var smallest = allCycles.Where(c => c.Count == minSize).ToList();
            List<GraphNode> cycle = smallest[Rng.Next(smallest.Count)];

            // Шаг 3: снимок ДО схлопывания
            steps.Add(new VizStep
            {
                Type = VizStepType.CycleFound,
                Description = $"Цикл из {cycle.Count} узлов: {string.Join(" → ", cycle.Select(n => n.Data.NodeType))}",
                Nodes = Snapshot(nodeSet.ToList()),
                HighlightIds = cycle.Select(GetId).ToList(),
                HighlightEdges = EdgesOf(cycle),
            });

            // Схлопываем
            var cycleSet = new HashSet<GraphNode>(cycle);
            GraphNode newNode = ProcessCycle(cycle);
            if (newNode == null) break;

            var external = new HashSet<GraphNode>();
            foreach (var node in cycle)
                foreach (var nb in node.GetNeighbours())
                    if (!cycleSet.Contains(nb))
                        external.Add(nb);

            foreach (var cn in cycle)
                if (cn != newNode) newNode.RemoveNeighbour(cn);

            foreach (var ext in external)
            {
                foreach (var cn in cycle) ext.RemoveNeighbour(cn);
                if (!newNode.GetNeighbours().Contains(ext)) newNode.AddNeighbour(ext);
                if (!ext.GetNeighbours().Contains(newNode)) ext.AddNeighbour(newNode);
            }

            var cur = graph.First;
            while (cur != null)
            {
                var next = cur.Next;
                if (cycleSet.Contains(cur.Value) && cur.Value != newNode)
                {
                    nodeSet.Remove(cur.Value);
                    nodeIndex.Remove(cur.Value);
                    sortedNodes.Remove(cur.Value);
                    graph.Remove(cur);
                }
                cur = next;
            }

            if (!nodeSet.Contains(newNode))
            {
                graph.AddLast(newNode);
                nodeSet.Add(newNode);
                nodeIndex[newNode] = nodeIndex.Count;
                sortedNodes.Add(newNode);
            }

            // Шаг 5: снимок ПОСЛЕ схлопывания
            steps.Add(new VizStep
            {
                Type = VizStepType.CycleCollapsed,
                Description = $"Схлопнуто → {newNode.Data.NodeType} | dmg={newNode.Data.Damage:F1}",
                Nodes = Snapshot(nodeSet.ToList()),
                HighlightIds = new List<string> { GetId(newNode) },
                HighlightEdges = newNode.GetNeighbours()
                                        .Select(nb => (GetId(newNode), GetId(nb)))
                                        .ToList(),
            });
        }

        graphList.Clear();
        graphList.AddRange(graph);
    }

    // ── Шаг 6 ─────────────────────────────────────────────────────────────────

    private static void CollapseTreeWithStep(List<GraphNode> graphList, List<VizStep> steps)
    {
        if (graphList.Count == 0) return;

        GraphNode start = graphList.Count == 1
            ? graphList[0]
            : graphList.OrderByDescending(n => n.num).ThenBy(_ => Rng.Next()).First();

        var visited = new HashSet<GraphNode> { start };
        var queue = new Queue<GraphNode>();
        queue.Enqueue(start);

        GraphNode result = start;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current != start)
                result.Data = MixAlgorithms.MixNodes(result.Data, current.Data);
            foreach (var nb in current.GetNeighbours())
                if (visited.Add(nb)) queue.Enqueue(nb);
        }

        steps.Add(new VizStep
        {
            Type = VizStepType.TreeCollapsed,
            Description = $"Дерево свёрнуто | dmg={result.Data.Damage:F1} | {result.Data.GetDominantAttack()}",
            Nodes = Snapshot(new List<GraphNode> { result }),
            HighlightIds = new List<string> { GetId(result) },
            HighlightEdges = new List<(string, string)>(),
        });

        graphList.Clear();
        graphList.Add(result);
    }

    // ── Snapshot: List<GraphNode> → List<NodeModel> ───────────────────────────
    //
    // NodeModel.id = GetId(node) — стабилен на всё время Solve.
    // NodeModel.type = node.Data.NodeType — актуален на момент снимка.
    // NodeModel.weight = округлённый node.Data.Weight.
    // NodeModel.connectedIds — только рёбра внутри снимка (не за его пределы).

    private static List<NodeModel> Snapshot(List<GraphNode> nodes)
    {
        var inSet = new HashSet<GraphNode>(nodes);
        var result = new List<NodeModel>(nodes.Count);

        foreach (var node in nodes)
        {
            var model = new NodeModel(node.Data.NodeType)
            {
                id = GetId(node),
                type = node.Data.NodeType,
                weight = Mathf.RoundToInt(node.Data.Weight),
            };

            foreach (var nb in node.GetNeighbours())
                if (inSet.Contains(nb))
                    model.AddLink(GetId(nb));

            result.Add(model);
        }

        return result;
    }

    // ── Вспомогательные ───────────────────────────────────────────────────────

    private static List<GraphNode> CollectAllNodes(SpellGraph graph)
    {
        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();
        queue.Enqueue(graph.StartNode);
        visited.Add(graph.StartNode);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            foreach (var nb in cur.GetNeighbours())
                if (visited.Add(nb)) queue.Enqueue(nb);
        }
        return visited.ToList();
    }

    private static List<List<GraphNode>> SeparateGraph(SpellGraph graph)
    {
        var result = new List<List<GraphNode>>();
        foreach (var start in graph.StartNode.GetNeighbours())
            result.Add(Bfs(start, graph.StartNode));
        return result;
    }

    private static List<GraphNode> Bfs(GraphNode start, GraphNode exclude)
    {
        var visited = new HashSet<GraphNode> { start };
        var result = new List<GraphNode>();
        var queue = new Queue<GraphNode>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            result.Add(cur);
            foreach (var nb in cur.GetNeighbours())
                if (nb != exclude && visited.Add(nb)) queue.Enqueue(nb);
        }
        return result;
    }

    private static List<List<GraphNode>> FindAllCycles(
        HashSet<GraphNode> nodeSet,
        Dictionary<GraphNode, int> nodeIndex,
        List<GraphNode> sortedNodes)
    {
        var allCycles = new List<List<GraphNode>>();
        var seen = new HashSet<string>();
        var onPath = new HashSet<GraphNode>();
        foreach (var root in sortedNodes)
        {
            if (!nodeSet.Contains(root)) continue;
            onPath.Clear();
            DfsCollectAll(root, null, root, nodeSet, nodeIndex, onPath, allCycles, seen);
        }
        return allCycles;
    }

    private static void DfsCollectAll(
        GraphNode current, GraphNode parent, GraphNode root,
        HashSet<GraphNode> nodeSet, Dictionary<GraphNode, int> nodeIndex,
        HashSet<GraphNode> onPath, List<List<GraphNode>> result, HashSet<string> seen)
    {
        onPath.Add(current);
        foreach (var nb in current.GetNeighbours())
        {
            if (!nodeSet.Contains(nb)) continue;
            if (nb == root && onPath.Count >= 3)
            {
                string sig = string.Join(",",
                    onPath.Select(n => nodeIndex[n].ToString()).OrderBy(s => s));
                if (seen.Add(sig)) result.Add(new List<GraphNode>(onPath));
                continue;
            }
            if (nb != parent && !onPath.Contains(nb)
                && nodeIndex.TryGetValue(nb, out int nIdx) && nIdx >= nodeIndex[root])
                DfsCollectAll(nb, current, root, nodeSet, nodeIndex, onPath, result, seen);
        }
        onPath.Remove(current);
    }

    private static GraphNode ProcessCycle(List<GraphNode> cycle)
    {
        if (cycle.Count < 3) return null;
        float buff = MixAlgorithms.CountBuff(cycle);
        foreach (var node in cycle)
            MixAlgorithms.IncreaseValue(node.Data, null, buff);

        bool found = true;
        while (found)
        {
            found = false;
            for (int i = 0; i < cycle.Count - 1 && !found; i++)
                for (int j = i + 1; j < cycle.Count; j++)
                {
                    var newData = MixAlgorithms.CheckWorkpiece(cycle[i].Data, cycle[j].Data);
                    if (newData != null)
                    {
                        cycle[i].Data = newData;
                        cycle.RemoveAt(j);
                        found = true; break;
                    }
                }
        }

        var mixed = cycle[0];
        for (int i = 1; i < cycle.Count; i++)
            mixed.Data = MixAlgorithms.MixNodes(mixed.Data, cycle[i].Data);
        return mixed;
    }

    private static List<(string, string)> EdgesOf(List<GraphNode> nodes)
    {
        var inSet = new HashSet<GraphNode>(nodes);
        var edges = new List<(string, string)>();
        var seen = new HashSet<(string, string)>();
        foreach (var node in nodes)
        {
            string a = GetId(node);
            foreach (var nb in node.GetNeighbours())
            {
                if (!inSet.Contains(nb)) continue;
                string b = GetId(nb);
                var key = string.Compare(a, b, System.StringComparison.Ordinal) < 0
                    ? (a, b) : (b, a);
                if (seen.Add(key)) edges.Add(key);
            }
        }
        return edges;
    }
}