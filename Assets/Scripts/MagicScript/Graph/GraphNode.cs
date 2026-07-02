using System.Collections.Generic;

public class GraphNode
{
    public NodeBase Data;
    private List<GraphNode> _neighbours = new();

    public virtual float Weight { get; set; }
    public int num = 1;

    public IReadOnlyList<GraphNode> GetNeighbours() => _neighbours;

    public void AddNeighbour(GraphNode node)
    {
        if (!_neighbours.Contains(node))
            _neighbours.Add(node);
    }

    public void RemoveNeighbour(GraphNode node)
    {
        _neighbours.Remove(node);
    }
    public GraphNode Clone()
    {
        return new GraphNode
        {
            Data = Data.Clone(),
            num = num,
            Weight = Weight,
        };
    }

}