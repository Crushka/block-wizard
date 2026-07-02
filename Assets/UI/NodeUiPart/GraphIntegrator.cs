using System.Collections.Generic;
using UnityEngine;

public class GraphIntegrator : MonoBehaviour
{
    public SpellGraph ConvertToBackendGraph(List<NodeModel> uiNodes)
    {
        SpellGraph backendGraph = new SpellGraph();
        
        Dictionary<string, GraphNode> map = new Dictionary<string, GraphNode>();

        foreach (var uiNode in uiNodes)
        {
            NodeBase elementData = GetElementData(uiNode.type, uiNode.num);
            
            float baseWeight = elementData.Weight > 0 ? elementData.Weight : 1.0f;
            GraphNode backendNode = backendGraph.CreateNode(elementData, uiNode.num, baseWeight);
            
            map[uiNode.id] = backendNode;

            if (uiNode.type == ElementType.None)
                backendGraph.StartNode = backendNode;
        }

        foreach (var uiNode in uiNodes)
        {
            if (!map.ContainsKey(uiNode.id)) continue;

            foreach (var targetId in uiNode.connectedIds)
            {
                if (map.ContainsKey(targetId))
                {
                    backendGraph.Connect(map[uiNode.id], map[targetId]);
                }
            }
        }

        return backendGraph;
    }

    public static NodeBase GetElementData(ElementType nodeType, int num)
    {
        return nodeType switch
        {
            ElementType.None => new NoneElement(),
            ElementType.Fire => new FireNode(num),
            ElementType.Water => new WaterNode(num),
            ElementType.Earth => new EartNode(num),
            ElementType.Air => new AirNode(num),
            ElementType.Lightning => new LightningNode(num),
            ElementType.Cold => new ColdNode(num),
            ElementType.Steam => new SteamNode(num),
            ElementType.Ice => new ColdNode(num),
            ElementType.Lava => new LavaNode(num),
            ElementType.Sound => new SoundNode(num),
            ElementType.Fog => new FogNode(num),
            ElementType.Plasma => new PlasmaNode(num),
            ElementType.Acid => new AcidNode(num),

            _ => new NoneElement()
        };
    }
}