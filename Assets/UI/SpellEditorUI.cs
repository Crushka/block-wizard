using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет открытием/закрытием редактора и запуском визуализации.
/// </summary>
public class SpellEditorUI : MonoBehaviour
{
    [Header("Панели UI")]
    public GameObject editorPanel;

    [Header("Визуализация")]
    public GraphVisualizer graphVisualizer;   // ← новая панель визуализации

    [Header("Связи")]
    public BookInteraction bookInteraction;
    public PlayerController playerController;
    public CameraController cameraController;
    public SpellCaster spellCaster;

    private bool _isOpen = false;

    // ── Init ──────────────────────────────────────────────────────────────

    void Start() => SetEditorOpen(false);

    // ── Toggle ────────────────────────────────────────────────────────────

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

    // ── Кнопка «Показать алгоритм» ────────────────────────────────────────

    /// <summary>
    /// Привяжи к кнопке «Compile» / «Показать» в редакторе.
    /// Запускает Solve, прячет редактор, показывает визуализатор.
    /// Когда игрок нажмёт «Применить» внутри визуализатора — редактор закроется.
    /// </summary>
    public void ShowVisualization()
    {
        if (graphVisualizer == null)
        {
            Debug.LogError("[SpellEditorUI] GraphVisualizer не назначен!");
            // Фолбэк: просто применить без визуализации
            BuildAndApplySpell();
            SetEditorOpen(false);
            return;
        }

        SpellGraph calculationGraph = BuildCalculationGraph(NodeEditorManager.Instance?.Graph);
        if (calculationGraph == null) return;

        calculationGraph.CalculateWeight();

        var (result, steps) = SpellSolverDebug.Solve(calculationGraph);

        // Прячем редактор, показываем визуализатор
        editorPanel.SetActive(false);

        graphVisualizer.Show(steps, () =>
        {
            // Игрок нажал «Применить» — передаём результат в SpellCaster
            ApplyResult(result);
            SetEditorOpen(false);
        });
    }

    // ── Кнопка «Применить без визуализации» (оставлена для совместимости) ─

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

        // Используем Solve вместо SlowGraph — результат идентичен
        var (result, _) = SpellSolverDebug.Solve(calculationGraph);

        ApplyResult(result);
    }

    // ── Применить результат в SpellCaster ─────────────────────────────────

    private void ApplyResult(NodeBase result)
    {
        if (spellCaster == null)
        {
            Debug.LogWarning("[SpellEditorUI] SpellCaster не назначен.");
            return;
        }

        // SpellCaster.PrepareSpell ждёт SpellGraph —
        // передаём минимальный граф из одного узла с готовым результатом.
        // Внутри PrepareSpell вызывается SpellGraphSolver.SlowGraph,
        // поэтому оборачиваем result в граф который он пройдёт без изменений.
        var wrapGraph = WrapResultInGraph(result);
        spellCaster.PrepareSpell(wrapGraph);

        Debug.Log($"[SpellEditorUI] Применено: dmg={result.Damage:F1} " +
                  $"range={result.Range:F1} attack={result.GetDominantAttack()}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

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

    /// <summary>
    /// Оборачивает готовый NodeBase в минимальный SpellGraph:
    /// StartNode (None) → resultNode.
    /// SpellGraphSolver.SlowGraph пройдёт его без изменений.
    /// </summary>
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