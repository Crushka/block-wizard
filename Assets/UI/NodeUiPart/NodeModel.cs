using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeModel 
{
    public string id;
    public ElementType type; 
    public int num = int.MaxValue;
    public List<string> connectedIds = new List<string>();

    public Vector2 anchoredPosition;

    public NodeModel(ElementType t) 
    {
        id = System.Guid.NewGuid().ToString();
        type = t;

        if (type == ElementType.None) 
            num = 0;
    }

    public void AddLink(string targetId) 
    {
        if (!string.IsNullOrEmpty(targetId) && !connectedIds.Contains(targetId))
            connectedIds.Add(targetId);
    }
}

[System.Serializable]
public class GraphModel 
{
    public List<NodeModel> Nodes = new List<NodeModel>();

    public void RecalculateWeights() 
    {
        foreach (var node in Nodes)
            node.num = (node.type == ElementType.None) ? 0 : int.MaxValue;

        Queue<NodeModel> queue = new Queue<NodeModel>();
        var startNode = Nodes.Find(n => n.type == ElementType.None);
        
        if (startNode != null) 
            queue.Enqueue(startNode);

        while (queue.Count > 0) 
        {
            var current = queue.Dequeue();
            foreach (var neighborId in current.connectedIds) 
            {
                var neighbor = Nodes.Find(n => n.id == neighborId);
                if (neighbor != null && neighbor.num == int.MaxValue) 
                {
                    neighbor.num = current.num + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    
}