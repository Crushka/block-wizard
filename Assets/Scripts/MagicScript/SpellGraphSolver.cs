


//ничего не проверял, но выглядит правлиьно 
//  1. разделить граф на подграфы, относительно root - готово
//  2. найти циклы подграфа через алгоритм Хортона - готово
//  3. цикл обработать и вернуть 1 нод - готово
//  4. заменить все циклы на полученые ноды - готово
//  5. обрабаотать полученное дерево
//  6. обрбабоать оставшеяся дерево с main_root

using System.Collections.Generic;
using System.Linq;

internal class SpellGraphSolver
{
    static List<GraphNode> Bfs(GraphNode start, GraphNode mainNode)
    {
        if (start == null) return new List<GraphNode>();
        var visited = new HashSet<GraphNode>();
        var result = new List<GraphNode>();
        var queue = new Queue<GraphNode>();
        visited.Add(start);
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            GraphNode current = queue.Dequeue();
            result.Add(current);
            foreach (GraphNode neighbor in current.GetNeighbours())
            {
                if (!visited.Contains(neighbor) && neighbor != mainNode)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return result;
    }

    private static List<List<GraphNode>> SeparateGraph(SpellGraph graph)
    {
        IReadOnlyList<GraphNode> startsNode = graph.StartNode.GetNeighbours();
        List<List<GraphNode>> result = new List<List<GraphNode>>();
        foreach (GraphNode start in startsNode)
        {
            result.Add(Bfs(start, graph.StartNode));
        }
        return result;
    }
    private static List<List<GraphNode>> FindAllCycles(List<GraphNode> graph)
    {
        var cycles = new List<List<GraphNode>>();
        var nodeSet = new HashSet<GraphNode>(graph);

        foreach (GraphNode root in graph)
        {
            var stack = new Stack<(GraphNode node, GraphNode parent, List<GraphNode> path)>();
            stack.Push((root, null, new List<GraphNode> { root }));

            while (stack.Count > 0)
            {
                var (current, parent, path) = stack.Pop();

                foreach (GraphNode neighbor in current.GetNeighbours())
                {
                    if (!nodeSet.Contains(neighbor)) continue;

                    if (neighbor == root && path.Count >= 3)
                    {
                        var cycle = new List<GraphNode>(path);
                        if (!IsDuplicateCycle(cycles, cycle))
                            cycles.Add(cycle);
                        continue;
                    }

                    if (neighbor != parent && !path.Contains(neighbor) &&
                        graph.IndexOf(neighbor) >= graph.IndexOf(root))
                    {
                        var newPath = new List<GraphNode>(path) { neighbor };
                        stack.Push((neighbor, current, newPath));
                    }
                }
            }
        }

        cycles.Sort((a, b) => a.Count.CompareTo(b.Count));
        return cycles;
    }

    private static List<GraphNode> FindFirstCycle(List<GraphNode> graph)
    {
        var nodeSet = new HashSet<GraphNode>(graph);

        foreach (GraphNode root in graph)
        {
            var stack = new Stack<(GraphNode node, GraphNode parent, List<GraphNode> path)>();
            stack.Push((root, null, new List<GraphNode> { root }));

            while (stack.Count > 0)
            {
                var (current, parent, path) = stack.Pop();

                foreach (GraphNode neighbor in current.GetNeighbours())
                {
                    if (!nodeSet.Contains(neighbor)) continue;

                    if (neighbor == root && path.Count >= 3)
                        return path;

                    if (neighbor != parent && !path.Contains(neighbor) &&
                        graph.IndexOf(neighbor) >= graph.IndexOf(root))
                    {
                        var newPath = new List<GraphNode>(path) { neighbor };
                        stack.Push((neighbor, current, newPath));
                    }
                }
            }
        }

        return null;
    }


    private static bool IsDuplicateCycle(List<List<GraphNode>> existing, List<GraphNode> candidate)
    {
        var candidateSet = new HashSet<GraphNode>(candidate);
        foreach (var cycle in existing)
        {
            if (cycle.Count == candidate.Count && new HashSet<GraphNode>(cycle).SetEquals(candidateSet))
                return true;
        }
        return false;
    }

    
    private static GraphNode ProcessCycle(List<GraphNode> cycle)
    {
        if (cycle.Count < 3) return null;

        GraphNode new_node = new GraphNode();
        GraphNode mixedNode = cycle[0];
        float buff = MixAlgorithms.CountBuff(cycle);

        foreach (GraphNode graphNode in cycle)
        {
            MixAlgorithms.IncreaseValue(graphNode.Data, null, buff);
        }

        for (int i = 1; i < cycle.Count; i++)
        {
            mixedNode.Data = MixAlgorithms.MixNodes(mixedNode.Data, cycle[i].Data);
        }

        return mixedNode;
    }

    private static void CollapseSingleCycle(List<GraphNode> graph, List<GraphNode> cycle)
    {
        var cycleSet = new HashSet<GraphNode>(cycle);

        var externalNeighbors = new HashSet<GraphNode>();
        foreach (GraphNode node in cycle)
        {
            foreach (GraphNode neighbor in node.GetNeighbours())
            {
                if (!cycleSet.Contains(neighbor))
                    externalNeighbors.Add(neighbor);
            }
        }

        GraphNode newNode = ProcessCycle(cycle);
        if (newNode == null) return;

        foreach (GraphNode external in externalNeighbors)
        {
            foreach (GraphNode cycleNode in cycle)
            {
                external.RemoveNeighbour(cycleNode); 
            }
            external.AddNeighbour(newNode);
            newNode.AddNeighbour(external);
        }

        foreach (GraphNode cycleNode in cycle)
            graph.Remove(cycleNode);

        graph.Add(newNode);
    }

    private static void CollapseCycles(List<GraphNode> graph)
    {
        int cycleCount = 0;
        while (true)
        {
            List<GraphNode> cycle = FindFirstCycle(graph);
            if (cycle == null) break;
            cycleCount++;
            CollapseSingleCycle(graph, cycle);
        }
    }

    public static NodeBase SlowGraph(SpellGraph graph)
    {
        List<List<GraphNode>> separateGraphs = SeparateGraph(graph);
        foreach (List<GraphNode> i in separateGraphs)
        {
            CollapseCycles(i);
        }

        return null;
    }
}