using System;
using System.Collections.Generic;
using System.Linq;


//без edge =(
public class SpellGraph
{
    public GraphNode StartNode;
    public List<GraphNode> Nodes = new();
    private Dictionary<ElementNode, int> Elements;
    public float Weight = 0.2f;
    public void Connect(GraphNode a, GraphNode b)
    {
        a.AddNeighbour(b);
        b.AddNeighbour(a);
    }

    public GraphNode CreateNode(NodeBase data)
    {
        var node = new GraphNode { Data = data };
        Nodes.Add(node);
        return node;
    }

    public void PrepareGraph()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            MixAlgorithms.IncreaseValue(Nodes[i].Data, null, Nodes[i].Data.Weight);
        }
    }

   
    public void CalculateWeight()
    {
        int n = Nodes.Count;
        int r = Elements.Values.Max();
        Weight = (float)(1 / (Math.Pow(n, Math.Sqrt(r * 0.5) / (Math.Max(1, n - r)))));
    }
}
