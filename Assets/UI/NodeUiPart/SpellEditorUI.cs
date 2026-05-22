using System.Collections.Generic;
using UnityEngine;

public class SpellEditorUI : MonoBehaviour
{
    [Header("Панели UI")]
    public GameObject editorPanel;

    [Header("Визуализация")]
    public GraphVisualizer graphVisualizer;

    [Header("Связи")]
    public BookInteraction bookInteraction;
    public PlayerController playerController;
    public CameraController cameraController;
    public SpellCaster spellCaster;

    private bool _isOpen = false;


    void Start() => SetEditorOpen(false);

    public void Toggle() => SetEditorOpen(!_isOpen);

    public void SetEditorOpen(bool open)
    {
        _isOpen = open;

        if (editorPanel != null)
            editorPanel.SetActive(open);
        else
            Debug.LogError("[SpellEditorUI] editorPanel == null");

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (playerController != null) playerController.isMovementEnabled = !open;
        if (cameraController != null) cameraController.isControlEnabled = !open;
    }

    public void ShowVisualization()
    {
        if (graphVisualizer == null)
        {
            Debug.LogError("[SpellEditorUI] GraphVisualizer не назначен!");
            BuildAndApplySpell();
            SetEditorOpen(false);
            return;
        }

        SpellGraph calculationGraph = BuildCalculationGraph(NodeEditorManager.Instance?.Graph);
        if (calculationGraph == null) return;

        calculationGraph.CalculateWeight();

        var (result, steps) = SpellSolverDebug.Solve(calculationGraph);
        editorPanel.SetActive(false);

        graphVisualizer.Show(steps, () =>
        {
            ApplyResult(result);
            SetEditorOpen(false);
        });
    }

    public void CastAndClose()
    {
        BuildAndApplySpell();
        SetEditorOpen(false);
    }

    public void BuildAndApplySpell()
    {
        SpellGraph calculationGraph = BuildCalculationGraph(NodeEditorManager.Instance?.Graph);
        if (calculationGraph == null) return;

        calculationGraph.CalculateWeight();

        var (result, _) = SpellSolverDebug.Solve(calculationGraph);

        ApplyResult(result);
    }


    private void ApplyResult(NodeBase result)
    {
        if (spellCaster == null)
        {
            Debug.LogWarning("[SpellEditorUI] SpellCaster не назначен.");
            return;
        }

        var wrapGraph = WrapResultInGraph(result);
        spellCaster.PrepareSpell(wrapGraph);

        Debug.Log($"[SpellEditorUI] Применено: dmg={result.Damage:F1} " +
                  $"range={result.Range:F1} attack={result.GetDominantAttack()}");
    }


    private SpellGraph BuildCalculationGraph(GraphModel uiGraph)
    {
        if (uiGraph == null || uiGraph.Nodes.Count == 0)
        {
            Debug.LogWarning("[SpellEditorUI] Граф пуст.");
            return null;
        }

        var graph = new SpellGraph();
        var map = new Dictionary<string, GraphNode>();
        var processed = new HashSet<string>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            NodeBase data = uiGraph.GetElementData(uiNode.type);
            if (data == null) continue;

            int num = uiNode.weight == int.MaxValue ? 0 : uiNode.weight;
            float baseW = data.Weight > 0 ? data.Weight : 1f;
            GraphNode gn = graph.CreateNode(data, num, baseW);
            map[uiNode.id] = gn;

            if (uiNode.type == ElementType.None)
                graph.StartNode = gn;
        }

        foreach (var uiNode in uiGraph.Nodes)
        {
            foreach (var targetId in uiNode.connectedIds)
            {
                string key = string.Compare(uiNode.id, targetId) < 0
                    ? uiNode.id + "_" + targetId
                    : targetId + "_" + uiNode.id;

                if (!processed.Contains(key)
                    && map.ContainsKey(uiNode.id)
                    && map.ContainsKey(targetId))
                {
                    graph.Connect(map[uiNode.id], map[targetId]);
                    processed.Add(key);
                }
            }
        }

        if (graph.StartNode == null)
        {
            Debug.LogError("[SpellEditorUI] Нет стартового узла (ElementType.None)!");
            return null;
        }

        return graph;
    }

    private SpellGraph WrapResultInGraph(NodeBase result)
    {
        var graph = new SpellGraph();
        var startNode = graph.CreateNode(new NoneElement(), 0, 1f);
        var resultNode = graph.CreateNode(result, 1, result.Weight);
        graph.StartNode = startNode;
        graph.Connect(startNode, resultNode);
        graph.CalculateWeight();
        return graph;
    }
}