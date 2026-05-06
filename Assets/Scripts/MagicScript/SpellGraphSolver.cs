using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

internal class SpellGraphSolver
{
    private static readonly System.Random Rng = new System.Random();
    private const int MaxCollapseIterations = 250;

    private static List<GraphNode> Bfs(GraphNode start, GraphNode mainNode)
    {
        if (start == null) return new List<GraphNode>();

        var visited = new HashSet<GraphNode> { start };
        var result = new List<GraphNode>();
        var queue = new Queue<GraphNode>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            GraphNode current = queue.Dequeue();
            result.Add(current);
            foreach (GraphNode neighbor in current.GetNeighbours())
                if (neighbor != mainNode && visited.Add(neighbor))
                    queue.Enqueue(neighbor);
        }
        return result;
    }

    private static List<List<GraphNode>> SeparateGraph(SpellGraph graph)
    {
        var result = new List<List<GraphNode>>();
        foreach (GraphNode start in graph.StartNode.GetNeighbours())
            result.Add(Bfs(start, graph.StartNode));
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

        foreach (GraphNode root in sortedNodes)
        {
            if (!nodeSet.Contains(root)) continue;
            onPath.Clear();
            DfsCollectAll(root, null, root, nodeSet, nodeIndex, onPath, allCycles, seen);
        }

        return allCycles;
    }
    private static void DfsCollectAll(
    GraphNode current,
    GraphNode parent,
    GraphNode root,
    HashSet<GraphNode> nodeSet,
    Dictionary<GraphNode, int> nodeIndex,
    HashSet<GraphNode> onPath,
    List<List<GraphNode>> result,
    HashSet<string> seen)
    {
        onPath.Add(current);

        foreach (GraphNode neighbor in current.GetNeighbours())
        {
            if (!nodeSet.Contains(neighbor)) continue;

            if (neighbor == root && onPath.Count >= 3)
            {
                var ids = onPath
                    .Select(n => nodeIndex[n].ToString())
                    .OrderBy(s => s);
                string sig = string.Join(",", ids);

                if (seen.Add(sig))
                    result.Add(new List<GraphNode>(onPath));

                continue;
            }

            if (neighbor != parent
                && !onPath.Contains(neighbor)
                && nodeIndex.TryGetValue(neighbor, out int nIdx)
                && nIdx >= nodeIndex[root])
            {
                DfsCollectAll(neighbor, current, root, nodeSet, nodeIndex, onPath, result, seen);
            }
        }

        onPath.Remove(current);

    }

    private static GraphNode ProcessCycle(List<GraphNode> cycle)
    {
        if (cycle.Count < 3) return null;

        float buff = MixAlgorithms.CountBuff(cycle);
        foreach (GraphNode node in cycle)
            MixAlgorithms.IncreaseValue(node.Data, null, buff);

        bool foundReaction = true;
        while (foundReaction)
        {
            foundReaction = false;
            for (int i = 0; i < cycle.Count - 1; i++)
            {
                for (int j = i + 1; j < cycle.Count; j++)
                {
                    NodeBase new_data = MixAlgorithms.CheckWorkpiece(cycle[i].Data, cycle[j].Data);
                    if (new_data != null)
                    {
                        cycle[i].Data = new_data;
                        cycle.RemoveAt(j);
                        foundReaction = true;
                        break;
                    }
                }
                if (foundReaction) break;
            }
        }

        GraphNode mixed = cycle[0];
        for (int i = 1; i < cycle.Count; i++)
        {
            mixed.Data = MixAlgorithms.MixNodes(mixed.Data, cycle[i].Data);
        }

        return mixed;
    }

    private static void CollapseSingleCycle(
        LinkedList<GraphNode> graph,
        HashSet<GraphNode> nodeSet,
        Dictionary<GraphNode, int> nodeIndex,
        List<GraphNode> sortedNodes,
        List<GraphNode> cycle)
    {
        var cycleSet = new HashSet<GraphNode>(cycle);
        GraphNode newNode = ProcessCycle(cycle);
        if (newNode == null) return;

        var externalNeighbors = new HashSet<GraphNode>();
        foreach (GraphNode node in cycle)
            foreach (GraphNode neighbor in node.GetNeighbours())
                if (!cycleSet.Contains(neighbor))
                    externalNeighbors.Add(neighbor);

        foreach (GraphNode cycleNode in cycle)
            if (cycleNode != newNode)
                newNode.RemoveNeighbour(cycleNode);

        foreach (GraphNode external in externalNeighbors)
        {
            foreach (GraphNode cycleNode in cycle)
                external.RemoveNeighbour(cycleNode);

            if (!newNode.GetNeighbours().Contains(external))
                newNode.AddNeighbour(external);
            if (!external.GetNeighbours().Contains(newNode))
                external.AddNeighbour(newNode);
        }

        var node_ = graph.First;
        while (node_ != null)
        {
            var next = node_.Next;
            if (cycleSet.Contains(node_.Value) && node_.Value != newNode)
            {
                nodeSet.Remove(node_.Value);
                nodeIndex.Remove(node_.Value);
                sortedNodes.Remove(node_.Value);
                graph.Remove(node_);
            }
            node_ = next;
        }

        if (!nodeSet.Contains(newNode))
        {
            graph.AddLast(newNode);
            nodeSet.Add(newNode);
            nodeIndex[newNode] = nodeIndex.Count;
            sortedNodes.Add(newNode);
        }
    }

    private static void CollapseCycles(List<GraphNode> graphList)
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
            List<List<GraphNode>> allCycles = FindAllCycles(nodeSet, nodeIndex, sortedNodes);
            //UnityEngine.Debug.Log($" {allCycles.Count}");

            if (allCycles.Count == 0) break;

            int minSize = allCycles.Min(c => c.Count);
            List<List<GraphNode>> smallest = allCycles.Where(c => c.Count == minSize).ToList();
            List<GraphNode> cycle = smallest[Rng.Next(smallest.Count)];

            //for (int i = 0; i < cycle.Count; i++)
            //    UnityEngine.Debug.Log($"cycle dmg: {cycle[i].Data.Damage}");

            CollapseSingleCycle(graph, nodeSet, nodeIndex, sortedNodes, cycle);
        }

        graphList.Clear();
        graphList.AddRange(graph);
    }

    private static GraphNode CollapseTree(List<GraphNode> graph)
    {
        if (graph.Count == 0) return null;
        if (graph.Count == 1) return graph[0];

        GraphNode start = graph
            .OrderByDescending(n => n.num)
            .ThenBy(_ => Rng.Next())
            .First();

        var visited = new HashSet<GraphNode> { start };
        var queue = new Queue<GraphNode>();
        queue.Enqueue(start);

        GraphNode result = start;
        while (queue.Count > 0)
        {
            GraphNode current = queue.Dequeue();
            if (current != start)
                result.Data = MixAlgorithms.MixNodes(result.Data, current.Data);

            foreach (GraphNode neighbor in current.GetNeighbours())
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
        }

        return result;
    }

    private static void NormalizeFinalNode(GraphNode node, float graphWeight)
    {
        
        MixAlgorithms.IncreaseValue(node.Data, null, graphWeight);
        MixAlgorithms.IncreaseValue(node.Data, null, node.Data.Weight);
        //UnityEngine.Debug.Log(node.Data.Weight);
        //UnityEngine.Debug.Log(node.Data.Damage);

    }

    public static NodeBase SlowGraph(SpellGraph graph)
    {
        graph.PrepareGraph();
        GraphNode finalSpell;
        List<List<GraphNode>> separateGraphs = SeparateGraph(graph);
        var collapsedNodes = new List<GraphNode>(separateGraphs.Count);

        foreach (List<GraphNode> subgraph in separateGraphs)
        {
            CollapseCycles(subgraph);
            GraphNode collapsed = CollapseTree(subgraph);
            UnityEngine.Debug.Log($"dmg: {collapsed.Data.Damage}");
            if (collapsed != null)
                collapsedNodes.Add(collapsed);
        }

        finalSpell = collapsedNodes[0];
        for (int i = 1; i < collapsedNodes.Count; i++)
            finalSpell.Data = MixAlgorithms.MixNodes(finalSpell.Data, collapsedNodes[i].Data);

        NormalizeFinalNode(finalSpell, graph.Weight);

        return finalSpell.Data;
    }
}