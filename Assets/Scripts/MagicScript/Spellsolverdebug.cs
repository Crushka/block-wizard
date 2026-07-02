using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VizStepType
{ 
    SubgraphSplit,  
    CycleFound,     
    CycleCollapsed,  
    TreeMixStep,    
    TreeCollapsed, 
    FinalResult, 
}

public class VizStep
{
    public VizStepType Type;
    public string Description;
    public List<NodeModel> Nodes;
    public List<string> HighlightIds;
    public List<(string, string)> HighlightEdges;
    public string MixSourceId;   
    public string MixTargetId;   
}

public static class SpellSolverDebug
{
    private static readonly System.Random Rng = new System.Random();
    private const int MaxCollapseIterations = 90;

    private static Dictionary<NodeBase, string> _dataToUiIds = new Dictionary<NodeBase, string>();

    private static string GetId(GraphNode node)
    {
        if (node == null || node.Data == null) return "unknown_node";
        if (node.Data.NodeType == ElementType.None) return "START_NODE";

        if (_dataToUiIds.TryGetValue(node.Data, out string id))
        {
            return id;
        }
        return $"unregistered_{node.Data.NodeType}";
    }

    public static void ResetAndRegisterIds(Dictionary<NodeBase, string> initialMap)
    {
        _dataToUiIds = new Dictionary<NodeBase, string>(initialMap);
    }

    public static (NodeBase result, List<VizStep> steps) Solve(SpellGraph _graph)
    {
        var graph = _graph.Clone();
        for (int i = 0; i < graph.Nodes.Count(); i++)
        {
            Debug.Log($"{graph.Nodes[i].Data.NodeType} {graph.Nodes[i].num} {graph.Nodes[i].Data.Weight}");
        }
        var steps = new List<VizStep>();
        //graph.PrepareGraph();
        var allNodes = CollectAllNodes(graph);
        var allAliveNodesInSolver = new HashSet<GraphNode>(allNodes);

        steps.Add(new VizStep
        {
            Type = VizStepType.SubgraphSplit,
            Description = "Исходное состояние: Анализ структуры магического графа",
            Nodes = Snapshot(allAliveNodesInSolver.ToList()),
            HighlightIds = new List<string> { "START_NODE" },
            HighlightEdges = new List<(string, string)>()
        });
        var separateGraphs = SeparateGraph(graph);

        if (graph.StartNode != null)
        {
            var neighbors = graph.StartNode.GetNeighbours().ToList();
            foreach (var nb in neighbors)
            {
                graph.StartNode.RemoveNeighbour(nb);
                nb.RemoveNeighbour(graph.StartNode);
            }
            
            allAliveNodesInSolver.Remove(graph.StartNode);
        }


        steps.Add(new VizStep
        {
            Type = VizStepType.SubgraphSplit,
            Description = $"Стартовая нода удалена. Граф разделен на {separateGraphs.Count} автономных веток",
            Nodes = Snapshot(allAliveNodesInSolver.ToList()),
            HighlightIds = separateGraphs
                .SelectMany(sg => sg)
                .Where(n => n != graph.StartNode)
                .Select(GetId)
                .ToList(),
            HighlightEdges = new List<(string, string)>()
        });

        foreach (var subgraph in separateGraphs)
        {
            CollapseCyclesWithSteps(subgraph, steps, allAliveNodesInSolver);
            CollapseTreeWithStep(subgraph, steps, allAliveNodesInSolver);
        }

        var collapsedNodes = separateGraphs.Where(sg => sg.Count > 0).Select(sg => sg[0]).ToList();
        if (collapsedNodes.Count == 0) return (null, steps);

        GraphNode finalNode = collapsedNodes[0];

        for (int i = 1; i < collapsedNodes.Count; i++)
        {
            var mergingNode = collapsedNodes[i];
            string srcId = GetId(mergingNode);
            string dstId = GetId(finalNode);

            steps.Add(new VizStep
            {
                Type = VizStepType.TreeMixStep,
                Description = $"Слияние результатов веток: {mergingNode.Data.NodeType} ({srcId}) → {finalNode.Data.NodeType} ({dstId})",
                Nodes = Snapshot(allAliveNodesInSolver.ToList()),
                HighlightIds = new List<string> { dstId, srcId },
                HighlightEdges = new List<(string, string)>(),
                MixSourceId = srcId,
                MixTargetId = dstId
            });

            finalNode.Data = MixAlgorithms.MixNodes(finalNode.Data, mergingNode.Data);
            allAliveNodesInSolver.Remove(mergingNode);
        }
        Debug.Log($"[SpellSlover] graph weight: {graph.Weight} and node weight {finalNode.Data.Weight} and node type {finalNode.Data.NodeType}");
        //if (graph.Nodes.Count() > 2) { 
            
        //    MixAlgorithms.IncreaseValue(finalNode.Data, finalNode.Data.Weight);
        //}
        MixAlgorithms.IncreaseValue(finalNode.Data, graph.Weight);

        finalNode.Data.GetBaseComposition().Threshold();
        steps.Add(new VizStep
        {
            Type = VizStepType.FinalResult,
            Description = $"{finalNode.Data.GetDominantAttack()} | dmg={finalNode.Data.Damage:F1}",
            Nodes = Snapshot(new List<GraphNode> { finalNode }),
            HighlightIds = new List<string> { GetId(finalNode) },
            HighlightEdges = new List<(string, string)>(),
        });
        Debug.Log($"[SpellSlover] {finalNode.Data.GetBaseComposition().GetFirstEffect()}");
        return (finalNode.Data, steps);
    }

    private static void CollapseCyclesWithSteps(List<GraphNode> graphList, List<VizStep> steps, HashSet<GraphNode> globalAlive)
    {
        var graph = new LinkedList<GraphNode>(graphList);
        var nodeSet = new HashSet<GraphNode>(graphList);
        var nodeIndex = new Dictionary<GraphNode, int>(graphList.Count);
        for (int i = 0; i < graphList.Count; i++) nodeIndex[graphList[i]] = i;

        var sortedNodes = graphList.OrderByDescending(n => n.num).ThenBy(_ => Rng.Next()).ToList();
        int iterations = 0;

        while (iterations++ < MaxCollapseIterations)
        {
            var allCycles = FindAllCycles(nodeSet, nodeIndex, sortedNodes);
            if (allCycles.Count == 0) break;

            int minSize = allCycles.Min(c => c.Count);
            var smallest = allCycles.Where(c => c.Count == minSize).ToList();
            List<GraphNode> cycle = smallest[Rng.Next(smallest.Count)];

            steps.Add(new VizStep
            {
                Type = VizStepType.CycleFound,
                Description = $"Внутри ветки найден цикл из {cycle.Count} узлов",
                Nodes = Snapshot(globalAlive.ToList()),
                HighlightIds = cycle.Select(GetId).ToList(),
                HighlightEdges = EdgesOf(cycle),
            });

            var cycleSet = new HashSet<GraphNode>(cycle);
            GraphNode newNode = ProcessCycleWithSteps(cycle, steps, globalAlive);
            if (newNode == null) break;

            var external = new HashSet<GraphNode>();
            foreach (var node in cycle)
                foreach (var nb in node.GetNeighbours())
                    if (!cycleSet.Contains(nb)) external.Add(nb);

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

            steps.Add(new VizStep
            {
                Type = VizStepType.CycleCollapsed,
                Description = $"Цикл ветки схлопнут в узел {newNode.Data.NodeType} ({GetId(newNode)})",
                Nodes = Snapshot(globalAlive.ToList()),
                HighlightIds = new List<string> { GetId(newNode) },
                HighlightEdges = newNode.GetNeighbours().Select(nb => (GetId(newNode), GetId(nb))).ToList(),
            });
        }

        graphList.Clear();
        graphList.AddRange(graph);
    }

    private static void CollapseTreeWithStep(List<GraphNode> graphList, List<VizStep> steps, HashSet<GraphNode> globalAlive)
    {
        if (graphList.Count == 0) return;

        GraphNode start = graphList.Count == 1
            ? graphList[0]
            : graphList.OrderByDescending(n => n.num).ThenBy(_ => Rng.Next()).First();

        //Debug.Log($"[SlovGraph] start num:{start.num} type: {start.Data.NodeType}");

        var visited = new HashSet<GraphNode> { start };
        var queue = new Queue<GraphNode>();
        queue.Enqueue(start);

        GraphNode result = start;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current != start)
            {
                string srcId = GetId(current);
                string dstId = GetId(result);

                steps.Add(new VizStep
                {
                    Type = VizStepType.TreeMixStep,
                    Description = $"Линейный микс ветки: {current.Data.NodeType} ({srcId}) → {result.Data.NodeType} ({dstId})",
                    Nodes = Snapshot(globalAlive.ToList()),
                    HighlightIds = new List<string> { dstId, srcId },
                    HighlightEdges = new List<(string, string)> { (dstId, srcId) },
                    MixSourceId = srcId,
                    MixTargetId = dstId,
                });

                result.Data = MixAlgorithms.MixNodes(result.Data, current.Data);
                globalAlive.Remove(current);
            }

            foreach (var nb in current.GetNeighbours())
                if (visited.Add(nb)) queue.Enqueue(nb);
        }

        steps.Add(new VizStep
        {
            Type = VizStepType.TreeCollapsed,
            Description = $"Ветка полностью свернута в узел ({GetId(result)})",
            Nodes = Snapshot(globalAlive.ToList()),
            HighlightIds = new List<string> { GetId(result) },
            HighlightEdges = new List<(string, string)>(),
        });

        graphList.Clear();
        graphList.Add(result);
    }

    private static GraphNode ProcessCycleWithSteps(List<GraphNode> cycle, List<VizStep> steps, HashSet<GraphNode> globalAlive)
    {
        if (cycle.Count < 3) return null;
        
        float buff = MixAlgorithms.CountBuff(cycle);
        foreach (var node in cycle) MixAlgorithms.IncreaseValue(node.Data, buff);

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
                        string srcId = GetId(cycle[j]);
                        string dstId = GetId(cycle[i]);

                        steps.Add(new VizStep
                        {
                            Type = VizStepType.TreeMixStep,
                            Description = $"Слияние заготовок цикла: {cycle[j].Data.NodeType} → {cycle[i].Data.NodeType}",
                            Nodes = Snapshot(globalAlive.ToList()),
                            HighlightIds = new List<string> { dstId, srcId },
                            HighlightEdges = new List<(string, string)> { (dstId, srcId) },
                            MixSourceId = srcId,
                            MixTargetId = dstId
                        });

                        _dataToUiIds[newData] = dstId;

                        cycle[i].Data = newData;
                        globalAlive.Remove(cycle[j]);
                        cycle.RemoveAt(j);
                        found = true; 
                        break;
                    }
                }
        }

        var mixed = cycle[0];
        string mainId = GetId(mixed);

        for (int i = 1; i < cycle.Count; i++)
        {
            var currentBlended = cycle[i];
            string blendId = GetId(currentBlended);

            steps.Add(new VizStep
            {
                Type = VizStepType.TreeMixStep,
                Description = $"Поглощение петли: {currentBlended.Data.NodeType} ({blendId}) → {mixed.Data.NodeType} ({mainId})",
                Nodes = Snapshot(globalAlive.ToList()),
                HighlightIds = new List<string> { mainId, blendId },
                HighlightEdges = new List<(string, string)> { (mainId, blendId) },
                MixSourceId = blendId,
                MixTargetId = mainId
            });

            var nextData = MixAlgorithms.MixNodes(mixed.Data, currentBlended.Data);
            _dataToUiIds[nextData] = mainId;

            mixed.Data = nextData;
            globalAlive.Remove(currentBlended);
        }

        return mixed;
    }

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
                num = Mathf.RoundToInt(node.Data.Weight),
            };
            foreach (var nb in node.GetNeighbours())
                if (inSet.Contains(nb)) model.AddLink(GetId(nb));

            result.Add(model);
        }
        return result;
    }

    private static List<GraphNode> CollectAllNodes(SpellGraph graph)
    {
        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();
        if (graph.StartNode == null) return new List<GraphNode>();
        
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

    private static List<List<GraphNode>> FindAllCycles(HashSet<GraphNode> nodeSet, Dictionary<GraphNode, int> nodeIndex, List<GraphNode> sortedNodes)
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

    private static void DfsCollectAll(GraphNode current, 
        GraphNode parent, 
        GraphNode root, 
        HashSet<GraphNode> nodeSet, 
        Dictionary<GraphNode, int> nodeIndex, 
        HashSet<GraphNode> onPath, 
        List<List<GraphNode>> result, 
        HashSet<string> seen)
    {
        onPath.Add(current);
        foreach (var nb in current.GetNeighbours())
        {
            if (!nodeSet.Contains(nb)) continue;
            if (nb == root && onPath.Count >= 3)
            {
                string sig = string.Join(",", onPath.Select(n => nodeIndex[n].ToString()).OrderBy(s => s));
                if (seen.Add(sig)) result.Add(new List<GraphNode>(onPath));
                continue;
            }
            if (nb != parent && !onPath.Contains(nb) && nodeIndex.TryGetValue(nb, out int nIdx) && nIdx >= nodeIndex[root])
                DfsCollectAll(nb, current, root, nodeSet, nodeIndex, onPath, result, seen);
        }
        onPath.Remove(current);
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
                var key = string.Compare(a, b, System.StringComparison.Ordinal) < 0 ? (a, b) : (b, a);
                if (seen.Add(key)) edges.Add(key);
            }
        }
        return edges;
    }
}