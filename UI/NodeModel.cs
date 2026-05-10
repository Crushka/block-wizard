using System.Collections.Generic;
using UnityEngine;

public enum MagicType { Fire, Water, Earth, Air, Magic }

[System.Serializable]
public class NodeModel
{
    public string id;
    public MagicType type;
    public int weight = int.MaxValue;
    public List<string> connectedIds = new List<string>();

    public NodeModel(MagicType t)
    {
        id = System.Guid.NewGuid().ToString();
        type = t;
        if (type == MagicType.Magic) weight = 0;
    }

    public void AddLink(string targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return;
        
        // Если этого ID еще нет в списке, добавляем его
        if (!connectedIds.Contains(targetId))
        {
            connectedIds.Add(targetId);
            Debug.Log($"ID {targetId} добавлен в связи. Всего связей: {connectedIds.Count}");
        }
    }
}

[System.Serializable]
public class GraphModel
{
    public List<NodeModel> Nodes = new List<NodeModel>();

    public void RecalculateWeights()
    {
        foreach (var node in Nodes)
            node.weight = (node.type == MagicType.Magic) ? 0 : int.MaxValue;

        Queue<NodeModel> queue = new Queue<NodeModel>();
        var magicNode = Nodes.Find(n => n.type == MagicType.Magic);
        if (magicNode != null) queue.Enqueue(magicNode);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighborId in current.connectedIds)
            {
                var neighbor = Nodes.Find(n => n.id == neighborId);
                if (neighbor != null && neighbor.weight == int.MaxValue)
                {
                    neighbor.weight = current.weight + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}