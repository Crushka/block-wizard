using System.Collections.Generic;

public class SpellGraph
{
    public GraphNode StartNode;
    public List<GraphNode> Nodes = new();

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
}
