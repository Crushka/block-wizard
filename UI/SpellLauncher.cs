using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpellCasterButton : MonoBehaviour
{
    public void CastSpellFromEditor()
    {
        if (NodeEditorManager.Instance == null)
        {
            NodeEditorManager.Instance = FindObjectOfType<NodeEditorManager>();
        }

        if (NodeEditorManager.Instance == null)
        {
            Debug.LogError("КРИТИЧЕСКАЯ ОШИБКА: Объект с компонентом NodeEditorManager не найден на сцене! Проверь Hierarchy.");
            return;
        }

        var uiGraph = NodeEditorManager.Instance.Graph;
        if (uiGraph == null || uiGraph.Nodes.Count == 0)
        {
            Debug.LogWarning("Граф пуст или не инициализирован.");
            return;
        }

        Debug.Log("Данные успешно получены, начинаем сборку расчетного графа...");

        SpellGraph calculationGraph = new SpellGraph();
        Dictionary<string, GraphNode> map = new Dictionary<string, GraphNode>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            NodeBase elementData = uiGraph.GetElementData(uiNode.type);

            if (elementData == null && uiNode.type != ElementType.None) continue;

            float weight = (uiNode.weight == int.MaxValue) ? 1.0f : uiNode.weight;

            GraphNode newNode = calculationGraph.CreateNode(elementData, 0, weight);
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
            return;
        }

        calculationGraph.CalculateWeight();
        NodeBase finalResult = SpellGraphSolver.SlowGraph(calculationGraph);
    }

    
}