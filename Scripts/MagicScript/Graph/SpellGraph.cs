using System;
using System.Collections.Generic;
using System.Linq;


//��� edge =(
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
            MixAlgorithms.IncreaseValue(Nodes[i].Data, null, Nodes[i].Weight);
        }
    }


    public void CalculateWeight()
    {
        CalculateElements();
        
        int n = Nodes.Count;
        if (Elements == null || Elements.Count == 0)
        {
            Weight = 1.0f;
            return;
        }

        int r = Elements.Values.Max();

        if (n <= 0) 
        {
            Weight = 1.0f;
            return;
        }

        Weight = (float)(1 / (Math.Pow(n, Math.Sqrt(r * 0.5) / (Math.Max(1, n - r)))));
    }
}
