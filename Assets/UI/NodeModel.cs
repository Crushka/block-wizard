using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeModel 
{
    public string id;
    public ElementType type; 
    public int weight = int.MaxValue;
    public List<string> connectedIds = new List<string>();

    public NodeModel(ElementType t) 
    {
        id = System.Guid.NewGuid().ToString();
        type = t;

        if (type == ElementType.None) 
            weight = 0;
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
            node.weight = (node.type == ElementType.None) ? 0 : int.MaxValue;

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
                if (neighbor != null && neighbor.weight == int.MaxValue) 
                {
                    neighbor.weight = current.weight + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public NodeBase GetElementData(ElementType nodeType)
    {
        return nodeType switch
        {
            ElementType.None      => new NoneElement(),
            ElementType.Fire      => new FireNode(),
            ElementType.Water     => new WaterNode(),
            ElementType.Earth     => new EartNode(),
            ElementType.Air       => new AirNode(),
            ElementType.Lightning => new LightningNode(),
            ElementType.Mud       => new MudNode(),
            ElementType.Steam     => new SteamNode(),
            ElementType.Plasma    => new PlasmaNode(),
            ElementType.Cold => new ColdNode(),

            _ => new NoneElement() 
        };
    }
}