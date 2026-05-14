using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpellCasterButton : MonoBehaviour
{
    public NodeBase CastSpellFromEditor()
    {
        if (NodeEditorManager.Instance == null)
        {
            NodeEditorManager.Instance = FindAnyObjectByType<NodeEditorManager>();
        }

        if (NodeEditorManager.Instance == null)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: Объект с компонентом NodeEditorManager не найден на сцене! Проверь Hierarchy.");
        }

        var uiGraph = NodeEditorManager.Instance.Graph;
        if (uiGraph == null || uiGraph.Nodes.Count == 0)
        {
            Debug.LogWarning("Граф пуст или не инициализирован.");
        }


        SpellGraph calculationGraph = new SpellGraph();
        Dictionary<string, GraphNode> map = new Dictionary<string, GraphNode>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            NodeBase elementData = uiGraph.GetElementData(uiNode.type);

            if (elementData == null && uiNode.type != ElementType.None) continue;

            int num = (uiNode.weight == int.MaxValue) ? 0 : uiNode.weight;
            float baseWeight = elementData.Weight > 0 ? elementData.Weight : 1.0f;

            GraphNode newNode = calculationGraph.CreateNode(elementData, num, baseWeight); // <-- num теперь правильный
            map.Add(uiNode.id, newNode);

            if (uiNode.type == ElementType.None)
                calculationGraph.StartNode = newNode;
        }

        HashSet<string> processedConnections = new HashSet<string>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            foreach (string targetId in uiNode.connectedIds)
            {
                string connectionKey = string.Compare(uiNode.id, targetId) < 0 
                    ? $"{uiNode.id}_{targetId}" 
                    : $"{targetId}_{uiNode.id}";

     
                if (!processedConnections.Contains(connectionKey))
                {
                    if (map.ContainsKey(uiNode.id) && map.ContainsKey(targetId))
                    {
                        calculationGraph.Connect(map[uiNode.id], map[targetId]);
                        processedConnections.Add(connectionKey);
                    }
                }
            }
        }

        if (calculationGraph.StartNode == null)
        {
            Debug.LogError("Ошибка: В графе отсутствует стартовый узел (ElementType.None)!");
        }

        calculationGraph.CalculateWeight();
        NodeBase finalResult = SpellGraphSolver.SlowGraph(calculationGraph);

        return finalResult;
    }

    
}