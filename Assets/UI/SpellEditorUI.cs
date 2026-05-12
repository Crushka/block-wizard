using UnityEngine;

/// <summary>
/// Управляет открытием/закрытием панели редактора заклинаний.
/// Вызывается из BookInteraction — сам Tab не слушает.
/// </summary>
public class SpellEditorUI : MonoBehaviour
{
    [Header("Панели UI")]
    [Tooltip("Корневой объект всего окна редактора (canvas или panel)")]
    public GameObject editorPanel;

    [Header("Связи")]
    public BookInteraction bookInteraction;
    public PlayerController playerController;
    public CameraController cameraController;
    public SpellCaster spellCaster;

    private bool _isOpen = false;

    // ── Init ──────────────────────────────────────────────────────────────

    void Start()
    {
        SetEditorOpen(false);
    }

    // ── Toggle ────────────────────────────────────────────────────────────

    public void Toggle() => SetEditorOpen(!_isOpen);

    /// <summary>Открыть/закрыть редактор. Можно вызвать из кнопки UI.</summary>
    public void SetEditorOpen(bool open)
    {
        _isOpen = open;


        if (editorPanel != null)
        {
            editorPanel.SetActive(open);
            Debug.Log($"[SpellEditorUI] editorPanel.activeSelf={editorPanel.activeSelf}");
        }
        else
            Debug.LogError("[SpellEditorUI] editorPanel == NULL! Назначь BookUI из Hierarchy в инспекторе GameManager.");

        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (playerController != null)
            playerController.isMovementEnabled = !open;

        if (cameraController != null)
            cameraController.isControlEnabled = !open;
    }

    // ── Cast (вызывается кнопкой внутри редактора) ────────────────────────

    /// <summary>
    /// Собирает граф из NodeEditorManager, передаёт в SpellCaster и закрывает редактор.
    /// Привяжи к кнопке «Cast» на панели.
    /// </summary>
    public void CastAndClose()
    {
        BuildAndApplySpell();
        SetEditorOpen(false);
    }

    /// <summary>
    /// Только применить заклинание без закрытия (если нужно).
    /// </summary>
    public void BuildAndApplySpell()
    {
        if (NodeEditorManager.Instance == null)
        {
            Debug.LogError("[SpellEditorUI] NodeEditorManager не найден!");
            return;
        }

        var uiGraph = NodeEditorManager.Instance.Graph;

        if (uiGraph == null || uiGraph.Nodes.Count == 0)
        {
            Debug.LogWarning("[SpellEditorUI] Граф пуст.");
            return;
        }

        // Строим расчётный граф (повторяет логику SpellLauncher)
        SpellGraph calculationGraph = BuildCalculationGraph(uiGraph);

        if (calculationGraph == null) return;

        calculationGraph.CalculateWeight();
        NodeBase result = SpellGraphSolver.SlowGraph(calculationGraph);

        Debug.Log($"[SpellEditorUI] Заклинание: dmg={result.Damage:F1} " +
                  $"range={result.Range:F1} speed={result.Speed:F1} " +
                  $"attack={result.GetDominantAttack()}");

        // Передаём в SpellCaster
        if (spellCaster != null)
        {
            var tempGraph = new SpellGraph();
            tempGraph.StartNode = new GraphNode { Data = result };
            spellCaster.PrepareSpell(calculationGraph);
        }
        else
        {
            Debug.LogWarning("[SpellEditorUI] SpellCaster не назначен — заклинание рассчитано, но не применено.");
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private SpellGraph BuildCalculationGraph(GraphModel uiGraph)
    {
        var graph = new SpellGraph();
        var map = new System.Collections.Generic.Dictionary<string, GraphNode>();
        var processed = new System.Collections.Generic.HashSet<string>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            NodeBase data = uiGraph.GetElementData(uiNode.type);
            if (data == null) continue;

            float w = (uiNode.weight == int.MaxValue) ? 1f : uiNode.weight;
            GraphNode gNode = graph.CreateNode(data, 0, w);
            map[uiNode.id] = gNode;

            if (uiNode.type == ElementType.None)
                graph.StartNode = gNode;
        }

        foreach (var uiNode in uiGraph.Nodes)
        {
            foreach (string targetId in uiNode.connectedIds)
            {
                string key = string.Compare(uiNode.id, targetId) < 0
                    ? uiNode.id + "_" + targetId
                    : targetId + "_" + uiNode.id;

                if (!processed.Contains(key) && map.ContainsKey(uiNode.id) && map.ContainsKey(targetId))
                {
                    graph.Connect(map[uiNode.id], map[targetId]);
                    processed.Add(key);
                }
            }
        }

        if (graph.StartNode == null)
        {
            Debug.LogError("[SpellEditorUI] Нет стартового узла (ElementType.None) в графе!");
            return null;
        }

        return graph;
    }
}