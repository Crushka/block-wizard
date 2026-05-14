using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Рендерит шаги визуализации алгоритма схлопывания.
///
/// Иерархия в Unity:
///   VisualizerPanel (этот компонент)
///     ├── VizGraphContainer   (RectTransform — сюда спавним NodeView)
///     ├── VizLineContainer    (RectTransform — сюда спавним ConnectionView)
///     ├── StepLabel           (TextMeshProUGUI — описание шага)
///     ├── StepCounter         (TextMeshProUGUI — "2 / 5")
///     ├── BtnNext             (Button)
///     ├── BtnPrev             (Button)
///     └── BtnAccept           (Button — видна только на последнем шаге)
/// </summary>
public class GraphVisualizer : MonoBehaviour
{
    [Header("Контейнеры")]
    public RectTransform graphContainer;
    public RectTransform lineContainer;

    [Header("Префабы")]
    public GameObject nodePrefab;
    public GameObject connectionPrefab;

    [Header("UI")]
    public TextMeshProUGUI stepLabel;
    public TextMeshProUGUI stepCounter;
    public Button btnNext;
    public Button btnPrev;
    public Button btnAccept;

    [Header("Цвета подсветки")]
    public Color highlightColor = new Color(1f, 0.85f, 0.1f);
    public Color normalColor = new Color(0.25f, 0.25f, 0.3f);
    public Color highlightEdge = new Color(1f, 0.7f, 0.1f);
    public Color normalEdge = new Color(0.5f, 0.5f, 0.5f);

    [Header("Анимация")]
    public float animDuration = 0.35f;

    // ── Состояние ──────────────────────────────────────────────────────────

    private List<VizStep> _steps;
    private int _currentIndex;
    private System.Action _onAccept;

    // Живые NodeView по id из снимка
    private Dictionary<string, NodeView> _nodeViews = new();
    // Живые ConnectionView по ключу "idA_idB"
    private Dictionary<string, ConnectionView> _edgeViews = new();

    // ── Публичный вход ─────────────────────────────────────────────────────

    /// <summary>
    /// Показать панель и запустить пошаговую визуализацию.
    /// onAccept вызывается когда игрок нажимает «Применить» на последнем шаге.
    /// </summary>
    public void Show(List<VizStep> steps, System.Action onAccept)
    {
        _steps = steps;
        _currentIndex = 0;
        _onAccept = onAccept;

        gameObject.SetActive(true);
        RenderStep(_currentIndex);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearAll();
    }

    // ── Кнопки (привяжи в инспекторе или через Start) ─────────────────────

    void Start()
    {
        btnNext?.onClick.AddListener(Next);
        btnPrev?.onClick.AddListener(Prev);
        btnAccept?.onClick.AddListener(Accept);
        gameObject.SetActive(false);
    }

    public void Next()
    {
        if (_currentIndex < _steps.Count - 1)
        {
            _currentIndex++;
            RenderStep(_currentIndex);
        }
    }

    public void Prev()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            RenderStep(_currentIndex);
        }
    }

    public void Accept()
    {
        Hide();
        _onAccept?.Invoke();
    }

    // ── Рендер одного шага ─────────────────────────────────────────────────

    private void RenderStep(int index)
    {
        var step = _steps[index];

        // 1. Обновляем UI-текст
        if (stepLabel != null) stepLabel.text = step.Description;
        if (stepCounter != null) stepCounter.text = $"{index + 1} / {_steps.Count}";

        btnPrev?.gameObject.SetActive(index > 0);
        btnNext?.gameObject.SetActive(index < _steps.Count - 1);
        btnAccept?.gameObject.SetActive(index == _steps.Count - 1);

        // 2. Синхронизируем NodeView
        SyncNodes(step);

        // 3. Синхронизируем рёбра
        SyncEdges(step);

        // 4. Подсвечиваем нужные
        ApplyHighlight(step);
    }

    // ── Синхронизация узлов ────────────────────────────────────────────────

    private void SyncNodes(VizStep step)
    {
        var stepIds = new HashSet<string>(step.Nodes.Select(n => n.id));

        // Удаляем NodeView которых нет в этом шаге
        var toRemove = _nodeViews.Keys.Where(id => !stepIds.Contains(id)).ToList();
        foreach (var id in toRemove)
        {
            Destroy(_nodeViews[id].gameObject);
            _nodeViews.Remove(id);
        }

        // Создаём или обновляем
        foreach (var model in step.Nodes)
        {
            if (_nodeViews.TryGetValue(model.id, out NodeView existing))
            {
                // Узел уже есть — просто обновляем данные (тип мог смениться после схлопывания)
                existing.Initialize(model);
            }
            else
            {
                // Новый узел
                var obj = Instantiate(nodePrefab, graphContainer);
                var view = obj.GetComponent<NodeView>();
                view.Initialize(model);
                view.SetVisualState(true);

                // Позиция: раскладываем по кругу / сетке
                obj.GetComponent<RectTransform>().anchoredPosition = ComputePosition(model, step.Nodes);

                _nodeViews[model.id] = view;

                // Анимация появления
                StartCoroutine(FadeIn(view.GetComponent<CanvasGroup>()));
            }
        }
    }

    // ── Синхронизация рёбер ────────────────────────────────────────────────

    private void SyncEdges(VizStep step)
    {
        // Собираем актуальный набор рёбер из connectedIds в снимке
        var currentEdgeKeys = new HashSet<string>();
        foreach (var model in step.Nodes)
        {
            foreach (var targetId in model.connectedIds)
            {
                string key = EdgeKey(model.id, targetId);
                currentEdgeKeys.Add(key);
            }
        }

        // Удаляем рёбра которых больше нет
        var toRemove = _edgeViews.Keys.Where(k => !currentEdgeKeys.Contains(k)).ToList();
        foreach (var key in toRemove)
        {
            Destroy(_edgeViews[key].gameObject);
            _edgeViews.Remove(key);
        }

        // Создаём новые рёбра
        foreach (var model in step.Nodes)
        {
            foreach (var targetId in model.connectedIds)
            {
                string key = EdgeKey(model.id, targetId);
                if (_edgeViews.ContainsKey(key)) continue;
                if (!_nodeViews.ContainsKey(model.id) || !_nodeViews.ContainsKey(targetId)) continue;

                var lineObj = Instantiate(connectionPrefab, lineContainer);
                var conn = lineObj.GetComponent<ConnectionView>();
                conn.Initialize(_nodeViews[model.id], _nodeViews[targetId], model.id, targetId);
                _edgeViews[key] = conn;
            }
        }
    }

    // ── Подсветка ──────────────────────────────────────────────────────────

    private void ApplyHighlight(VizStep step)
    {
        var hlNodeSet = new HashSet<string>(step.HighlightIds);
        var hlEdgeSet = new HashSet<string>(
            step.HighlightEdges.Select(e => EdgeKey(e.Item1, e.Item2)));

        foreach (var (id, view) in _nodeViews)
        {
            bool hl = hlNodeSet.Contains(id);
            var bg = view.GetComponent<Image>() ?? view.GetComponentInChildren<Image>();
            if (bg != null)
                StartCoroutine(AnimateColor(bg, bg.color, hl ? highlightColor : normalColor));
        }

        foreach (var (key, conn) in _edgeViews)
        {
            bool hl = hlEdgeSet.Contains(key);
            Image img = conn.lineImage != null ? conn.lineImage : conn.GetComponent<Image>();
            if (img != null)
                StartCoroutine(AnimateColor(img, img.color, hl ? highlightEdge : normalEdge));
        }
    }

    // ── Позиционирование ───────────────────────────────────────────────────

    /// <summary>
    /// Простая радиальная раскладка. Замени на свой алгоритм если нужно.
    /// Стартовый узел (None / Magic) всегда сверху, остальные по кругу.
    /// </summary>
    private Vector2 ComputePosition(NodeModel model, List<NodeModel> allNodes)
    {
        // Если у узла уже есть позиция в редакторе — найдём его NodeView
        var editorViews = NodeEditorManager.Instance != null
            ? NodeEditorManager.Instance.graphContainer
                  .GetComponentsInChildren<NodeView>()
                  .ToDictionary(v => v.Data.id)
            : new Dictionary<string, NodeView>();

        // Пытаемся взять позицию из живого редактора по типу (приблизительно)
        // Для схлопнутых узлов — усредняем позиции входящих
        // Простейший вариант: радиальная раскладка
        int total = allNodes.Count;
        int i = allNodes.IndexOf(model);
        float radius = Mathf.Min(graphContainer.rect.width, graphContainer.rect.height) * 0.35f;
        float angle = (360f / total) * i - 90f;
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
    }

    // ── Утилиты ────────────────────────────────────────────────────────────

    private void ClearAll()
    {
        foreach (var v in _nodeViews.Values) if (v) Destroy(v.gameObject);
        foreach (var e in _edgeViews.Values) if (e) Destroy(e.gameObject);
        _nodeViews.Clear();
        _edgeViews.Clear();
    }

    private static string EdgeKey(string a, string b)
        => string.Compare(a, b, System.StringComparison.Ordinal) < 0
            ? a + "_" + b
            : b + "_" + a;

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / animDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator AnimateColor(Image img, Color from, Color to)
    {
        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            img.color = Color.Lerp(from, to, t / animDuration);
            yield return null;
        }
        img.color = to;
    }
}