using System.Collections.Generic;

public class GraphNode
{
    public NodeBase Data;
    private List<GraphNode> _neighbours = new();

    public IReadOnlyList<GraphNode> GetNeighbours() => _neighbours;

    public void AddNeighbour(GraphNode node)
    {
        if (!_neighbours.Contains(node))
            _neighbours.Add(node);
    }

    public void RemoveNeighbour(GraphNode node)
    {
        _neighbours.Remove(node); // List<T>.Remove сам находит и удаляет первый
    }
}