using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class SpellCasterButton : MonoBehaviour
{
    [Header("Настройки анимации")]
    [Tooltip("Время задержки между шагами (в секундах)")]
    [SerializeField] private float stepDuration = 1.0f;

    [Header("Префабы элементов UI")]
    public GameObject nodePrefab;
    public GameObject connectionPrefab;

   
    [Header("Кнопки управления")]
    [Tooltip("Запуск анимации решения графа")]
    public Button buttonCast;

    [Tooltip("Пропустить анимацию, показать финальный шаг")]
    public Button buttonSkip;

    [Tooltip("Сбросить граф до одной стартовой ноды")]
    public Button buttonResetToStart;

    [Header("Характеристики заклинания")]
    [Tooltip("Урон (Damage), целое число")]
    public TMP_Text textDamage;

    [Tooltip("Скорость (Speed), целое число")]
    public TMP_Text textSpeed;

    [Tooltip("Дальность (Range), целое число")]
    public TMP_Text textRange;

   
    private Coroutine _activeAnimation;
    private bool _isProcessing = false;

    private List<VizStep> _currentSteps;
    private Dictionary<string, Vector2> _cachedPositions;
    private Vector2 _cachedCenter;

   
    private void Start()
    {
        if (buttonCast != null) buttonCast.onClick.AddListener(CastSpellFromEditor_Button);
        if (buttonSkip != null) buttonSkip.onClick.AddListener(SkipAnimation);
        if (buttonResetToStart != null) buttonResetToStart.onClick.AddListener(ResetToStartNode);

        UpdateStatTexts(null);
    }

 
    public void CastSpellFromEditor_Button() => CastSpellFromEditor();
    public NodeBase CastSpellFromEditor()
    {
        if (_isProcessing) return null;

        var manager = NodeEditorManager.Instance;
        if (manager == null || manager.Graph == null) return null;

        var uiGraph = manager.Graph;
        if (uiGraph.Nodes.Count == 0) return null;

        uiGraph.RecalculateWeights();

        SpellGraph calculationGraph = ConvertUIBranchToBackendAndRegister(uiGraph);
        if (calculationGraph.StartNode == null) return null;

        var (finalResult, vizSteps) = SpellSolverDebug.Solve(calculationGraph);
        if (vizSteps == null || vizSteps.Count == 0) return null;

        _currentSteps = vizSteps;

        UpdateStatTexts(finalResult);

        _activeAnimation = StartCoroutine(AnimateCollapseRoutine(vizSteps));
        return finalResult;
    }

  
    public void SkipAnimation()
    {
        if (!_isProcessing || _currentSteps == null || _currentSteps.Count == 0) return;

        if (_activeAnimation != null)
        {
            StopCoroutine(_activeAnimation);
            _activeAnimation = null;
        }

        var finalStep = _currentSteps[_currentSteps.Count - 1];

        if (_cachedPositions == null) _cachedPositions = new Dictionary<string, Vector2>();

        foreach (var step in _currentSteps)
        {
            if (step.Type == VizStepType.TreeMixStep &&
                !string.IsNullOrEmpty(step.MixSourceId) &&
                !string.IsNullOrEmpty(step.MixTargetId))
            {
                if (_cachedPositions.TryGetValue(step.MixTargetId, out var tp))
                    _cachedPositions[step.MixSourceId] = tp;
            }
        }

        var finalPositions = CalculateSmartPositions(finalStep, _cachedPositions, _cachedCenter);
        ApplyStepWithPositions(finalStep, finalPositions);

        _isProcessing = false;
        _currentSteps = null;
    }

    public void ResetToStartNode()
    {
        if (_activeAnimation != null) { StopCoroutine(_activeAnimation); _activeAnimation = null; }
        _isProcessing = false;
        _currentSteps = null;

        var manager = NodeEditorManager.Instance;
        if (manager == null) return;

        manager.Graph.Nodes.Clear();
        ClearUIContainers();

        var startModel = new NodeModel(ElementType.None) { num = 0 };
        manager.Graph.Nodes.Add(startModel);

        var startObj = Instantiate(nodePrefab, manager.graphContainer);
        var startView = startObj.GetComponent<NodeView>();
        startView.Initialize(startModel);
        startView.SetVisualState(true);
        startObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        manager.RefreshGraph();
        UpdateStatTexts(null);
    }

 
    private void UpdateStatTexts(NodeBase node)
    {
        if (textDamage != null) textDamage.text = node != null ? Mathf.RoundToInt(node.Damage).ToString() : "—";
        if (textSpeed != null) textSpeed.text = node != null ? Mathf.RoundToInt(node.Speed).ToString() : "—";
        if (textRange != null) textRange.text = node != null ? Mathf.RoundToInt(node.Range).ToString() : "—";
    }

    private IEnumerator AnimateCollapseRoutine(List<VizStep> steps)
    {
        _isProcessing = true;
        var manager = NodeEditorManager.Instance;

        var globalPositionCache = new Dictionary<string, Vector2>();
        var backupCenter = Vector2.zero;
        int count = 0;

        foreach (Transform child in manager.graphContainer)
        {
            var nv = child.GetComponent<NodeView>();
            if (nv != null && nv.Data != null)
            {
                var pos = child.GetComponent<RectTransform>().anchoredPosition;
                globalPositionCache[nv.Data.id] = pos;
                backupCenter += pos;
                count++;
            }
        }
        if (count > 0) backupCenter /= count;

        _cachedPositions = globalPositionCache;
        _cachedCenter = backupCenter;

        for (int i = 0; i < steps.Count; i++)
        {
            var currentStep = steps[i];

            if (i > 0 && globalPositionCache.ContainsKey("START_NODE"))
            {
                bool startAlive = currentStep.Nodes.Any(n => n.id == "START_NODE");
                if (!startAlive) globalPositionCache.Remove("START_NODE");
            }

            if (currentStep.Type == VizStepType.TreeMixStep &&
                !string.IsNullOrEmpty(currentStep.MixSourceId) &&
                !string.IsNullOrEmpty(currentStep.MixTargetId))
            {
                if (globalPositionCache.TryGetValue(currentStep.MixTargetId, out var tp))
                    globalPositionCache[currentStep.MixSourceId] = tp;
            }

            var positions = CalculateSmartPositions(currentStep, globalPositionCache, backupCenter);
            ApplyStepWithPositions(currentStep, positions);

            yield return new WaitForSeconds(stepDuration);
        }

        _isProcessing = false;
        _activeAnimation = null;
    }

    private void ApplyStepWithPositions(VizStep step, Dictionary<string, Vector2> positions)
    {
        var manager = NodeEditorManager.Instance;
        ClearUIContainers();

        var currentViews = new Dictionary<string, NodeView>();

        foreach (var nodeModel in step.Nodes)
        {
            var nObj = Instantiate(nodePrefab, manager.graphContainer);
            var view = nObj.GetComponent<NodeView>();
            view.Initialize(nodeModel);

            bool highlighted = step.HighlightIds != null && step.HighlightIds.Contains(nodeModel.id);
            view.SetVisualState(highlighted);

            var rt = view.GetComponent<RectTransform>();
            if (positions.TryGetValue(nodeModel.id, out var savedPos))
                rt.anchoredPosition = savedPos;

            currentViews[nodeModel.id] = view;
        }

        var renderedLines = new HashSet<string>();
        foreach (var nodeModel in step.Nodes)
        {
            foreach (var targetId in nodeModel.connectedIds)
            {
                if (!currentViews.ContainsKey(targetId)) continue;

                string lineKey = string.Compare(nodeModel.id, targetId, System.StringComparison.Ordinal) < 0
                    ? $"{nodeModel.id}_{targetId}" : $"{targetId}_{nodeModel.id}";

                if (renderedLines.Contains(lineKey)) continue;

                var lObj = Instantiate(connectionPrefab, manager.lineContainer);
                lObj.GetComponent<ConnectionView>().Initialize(
                    currentViews[nodeModel.id], currentViews[targetId], nodeModel.id, targetId);
                renderedLines.Add(lineKey);
            }
        }

        manager.RefreshLines();
    }

    private void ClearUIContainers()
    {
        var manager = NodeEditorManager.Instance;
        for (int i = manager.graphContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(manager.graphContainer.GetChild(i).gameObject);
        for (int i = manager.lineContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(manager.lineContainer.GetChild(i).gameObject);
    }

    private Dictionary<string, Vector2> CalculateSmartPositions(
        VizStep step, Dictionary<string, Vector2> globalCache, Vector2 backupCenter)
    {
        var calculated = new Dictionary<string, Vector2>();
        var unpositionedIds = new List<string>();

        foreach (var node in step.Nodes)
        {
            if (globalCache.TryGetValue(node.id, out var oldPos)) calculated[node.id] = oldPos;
            else unpositionedIds.Add(node.id);
        }

        if (unpositionedIds.Count > 0)
        {
            var collapseCenter = Vector2.zero;
            int highlightedCount = 0;

            if (step.HighlightIds != null)
                foreach (var id in step.HighlightIds)
                    if (globalCache.TryGetValue(id, out var pos)) { collapseCenter += pos; highlightedCount++; }

            if (highlightedCount > 0) collapseCenter /= highlightedCount;
            else collapseCenter = backupCenter;

            foreach (var newId in unpositionedIds)
            {
                calculated[newId] = collapseCenter;
                globalCache[newId] = collapseCenter;
            }
        }
        return calculated;
    }

    private SpellGraph ConvertUIBranchToBackendAndRegister(GraphModel uiGraph)
    {
        var graph = new SpellGraph();
        var map = new Dictionary<string, GraphNode>();
        var dataToIdMap = new Dictionary<NodeBase, string>();

        foreach (var uiNode in uiGraph.Nodes)
        {
            NodeBase elementData = GraphIntegrator.GetElementData(uiNode.type, uiNode.num);
            if (elementData == null && uiNode.type != ElementType.None) continue;

            int num = (uiNode.num == int.MaxValue) ? 0 : uiNode.num;
            float baseWeight = (elementData != null && elementData.Weight > 0) ? elementData.Weight : 1.0f;

            GraphNode newNode = graph.CreateNode(elementData, num, baseWeight);
            map[uiNode.id] = newNode;

            if (elementData != null) dataToIdMap[elementData] = uiNode.id;
            if (uiNode.type == ElementType.None) graph.StartNode = newNode;
        }

        SpellSolverDebug.ResetAndRegisterIds(dataToIdMap);

        var processed = new HashSet<string>();
        foreach (var uiNode in uiGraph.Nodes)
        {
            foreach (var targetId in uiNode.connectedIds)
            {
                string key = string.Compare(uiNode.id, targetId) < 0
                    ? $"{uiNode.id}_{targetId}" : $"{targetId}_{uiNode.id}";

                if (!processed.Contains(key) && map.ContainsKey(uiNode.id) && map.ContainsKey(targetId))
                {
                    graph.Connect(map[uiNode.id], map[targetId]);
                    processed.Add(key);
                }
            }
        }
        return graph;
    }
}