using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;


public class SpellGraph
{
    public GraphNode StartNode;
    public List<GraphNode> Nodes = new();
    private Dictionary<ElementType, int> Elements;
    public float Weight = 1.0f;
    public void Connect(GraphNode a, GraphNode b)
    {
        a.AddNeighbour(b);
        b.AddNeighbour(a);
    }

    public GraphNode CreateNode(NodeBase data, int number, float W)
    {
        var node = new GraphNode { Data = data, num = number, Weight = W };
        Nodes.Add(node);
        return node;
    }

    public void CalculateElements()
    {
        Elements = new Dictionary<ElementType, int>();

        foreach (GraphNode node in Nodes)
        {
            ElementType type = node.Data.NodeType;
            if (type == ElementType.None || type == ElementType.Unknown) continue;

            if (Elements.ContainsKey(type))
                Elements[type]++;
            else
                Elements[type] = 1;
        }
    }

    public void PrepareGraph()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            MixAlgorithms.IncreaseValue(Nodes[i].Data, Nodes[i].Weight);
        }
    }

    public void CalculateWeight()
    {
        CalculateElements();

        int n = Nodes.Count;
        int r = Elements.Values.Max();
        Weight = (float)(1 / (Math.Pow(n, Math.Sqrt(r * 0.5) / (Math.Max(1, n - r))))) + 0.43f;
    }

    public IEnumerable<GraphNode> AllNodes()
    {
        if (StartNode == null) yield break;

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();
        queue.Enqueue(StartNode);
        visited.Add(StartNode);

        while (queue.Count > 0)
        {
            GraphNode current = queue.Dequeue();
            yield return current;
            foreach (GraphNode neighbor in current.GetNeighbours())
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
        }
    }

    public SpellGraph Clone()
    {
        var copy = new SpellGraph { Weight = this.Weight };

        var map = new Dictionary<GraphNode, GraphNode>();
        foreach (var node in Nodes)
        {
            var cloned = node.Clone();
            map[node] = cloned;
            copy.Nodes.Add(cloned);
        }

        foreach (var node in Nodes)
            foreach (var nb in node.GetNeighbours())
                if (map.ContainsKey(nb))
                    map[node].AddNeighbour(map[nb]);

        if (StartNode != null && map.ContainsKey(StartNode))
            copy.StartNode = map[StartNode];

        return copy;
    }
}
