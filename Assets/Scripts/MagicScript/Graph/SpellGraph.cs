using System.Collections.Generic;

public class SpellGraph
{
    public GraphNode StartNode;
    public List<GraphNode> Nodes = new();
    public List<GraphEdge> Edges = new();

    public GraphEdge Connect(GraphNode a, GraphNode b)
    {
        var edge = new GraphEdge { From = a, To = b };
        a.Edges.Add(edge);
        b.Edges.Add(edge);
        Edges.Add(edge);
        return edge;
    }
}
