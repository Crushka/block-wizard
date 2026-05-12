using System.Collections.Generic;
using UnityEngine;

public class GraphIntegrator : MonoBehaviour
{
    public SpellGraph ConvertToBackendGraph(List<NodeModel> uiNodes)
    {
        SpellGraph backendGraph = new SpellGraph();
        // Словарь для связи: твой string ID -> GraphNode друга
        Dictionary<string, GraphNode> map = new Dictionary<string, GraphNode>();

        // 1. Создаем узлы
        foreach (var uiNode in uiNodes)
        {
            NodeBase elementData = CreateElementFromType(uiNode.type);
            
            // Используем твой weight как номер слоя (num), а базовый вес из данных элемента
            float baseWeight = elementData.Weight > 0 ? elementData.Weight : 1.0f;
            GraphNode backendNode = backendGraph.CreateNode(elementData, uiNode.weight, baseWeight);
            
            map[uiNode.id] = backendNode;

            if (uiNode.type == ElementType.Magic)
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

    private NodeBase CreateElementFromType(ElementType type)
    {
        switch (type)
        {
            case ElementType.Fire:      return new FireNode();
            case ElementType.Water:     return new WaterNode();
            case ElementType.Earth:     return new EartNode(); // Опечатка друга сохранена для компиляции
            case ElementType.Air:       return new AirNode();
            case ElementType.Magic:     return new NoneElement();
            default:                  return new NoneElement();
        }
    }
}