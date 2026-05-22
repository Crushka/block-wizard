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
            UnityEngine.Debug.Log(type);
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
            MixAlgorithms.IncreaseValue(Nodes[i].Data, null, Nodes[i].Weight);
        }
    }


    public void CalculateWeight()
    {
        CalculateElements();

        int n = Nodes.Count;
        int r = Elements.Values.Max();
        Weight = (float)(1 / (Math.Pow(n, Math.Sqrt(r * 0.5) / (Math.Max(1, n - r))))) + 0.4f;
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
}
