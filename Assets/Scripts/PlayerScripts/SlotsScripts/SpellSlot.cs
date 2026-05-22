using System.Collections.Generic;
using UnityEngine;

public class SpellSlot
{
    public int index;
    public GraphModel graph = null; 

    [System.NonSerialized]
    public NodeBase compiledNode = null; 

    public bool IsEmpty => graph == null || graph.Nodes.Count == 0;

    public SpellSlot(int idx) { index = idx; }

    public void SnapshotFromEditor(NodeEditorManager mgr)
    {
        if (mgr?.Graph == null) { graph = null; return; }

        graph = new GraphModel();

        // Находим все NodeView на холсте, чтобы узнать их текущие позиции
        var activeViews = mgr.graphContainer.GetComponentsInChildren<NodeView>();

        foreach (var src in mgr.Graph.Nodes)
        {
            Vector2 savedPos = Vector2.zero;

            // Ищем UI-компонент, соответствующий этой data-модели
            foreach (var view in activeViews)
            {
                if (view != null && view.Data == src)
                {
                    var rt = view.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        savedPos = rt.anchoredPosition;
                    }
                    break;
                }
            }

            var copy = new NodeModel(src.type)
            {
                id = src.id,
                weight = src.weight,
                anchoredPosition = savedPos // Сохраняем позицию!
            };

            foreach (var id in src.connectedIds) copy.AddLink(id);
            graph.Nodes.Add(copy);
        }
        Debug.Log($"[SpellSlot {index}] Снимок сделан: {graph.Nodes.Count} узлов");
    }

    public void RestoreToEditor(NodeEditorManager mgr)
    {
        if (mgr == null) return;

        // 1. ПОЛНАЯ ОЧИСТКА: убираем старый хлам и дюпы
        mgr.ClearEditor();

        // Если слот пустой, просто инициализируем чистый редактор со стартовой нодой
        if (graph == null || graph.Nodes.Count == 0)
        {
            mgr.InitEditor();
            Debug.Log($"[SpellSlot {index}] Пустой слот → редактор сброшен к дефолту");
            return;
        }

        // 2. ВОССТАНОВЛЕНИЕ: Сначала обрабатываем корневую ноду None
        var savedNone = graph.Nodes.Find(n => n.type == ElementType.None);

        // Спавним дефолтную стартовую структуру через менеджер
        mgr.InitEditor();

        // Находим только что созданную None-ноду в менеджере и синхронизируем её данные
        var liveNone = mgr.Graph.Nodes.Find(n => n.type == ElementType.None);
        if (liveNone != null && savedNone != null)
        {
            liveNone.id = savedNone.id;
            liveNone.weight = savedNone.weight;
            liveNone.anchoredPosition = savedNone.anchoredPosition;

            // Находим её визуальный объект и двигаем на сохраненную позицию
            var liveNoneView = mgr.graphContainer.GetComponentInChildren<NodeView>();
            if (liveNoneView != null)
            {
                liveNoneView.GetComponent<RectTransform>().anchoredPosition = savedNone.anchoredPosition;
            }
        }

        // 3. Спавним все остальные сохраненные ноды (кроме None, её мы уже настроили)
        foreach (var node in graph.Nodes)
        {
            if (node.type == ElementType.None) continue;

            // Добавляем в логический граф менеджера
            mgr.Graph.Nodes.Add(node);

            // Создаем визуальный объект на холсте
            var obj = Object.Instantiate(mgr.nodePrefab, mgr.graphContainer);

            // Сразу выставляем ей правильную сохраненную позицию на UI панели!
            var rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = node.anchoredPosition;
            }

            var view = obj.GetComponent<NodeView>();
            if (view != null)
            {
                view.Initialize(node);
                view.SetVisualState(true);
            }
        }

        // 4. Обновляем связи и перерисовываем линии
        mgr.RefreshGraph();
        Debug.Log($"[SpellSlot {index}] Граф успешно восстановлен: {graph.Nodes.Count} узлов");
    }

    public NodeBase Compile()
    {
        if (graph == null || graph.Nodes.Count == 0)
        {
            compiledNode = null;
            return null;
        }

        try
        {
            var calcGraph = new SpellGraph();
            var map = new System.Collections.Generic.Dictionary<string, GraphNode>();

            foreach (var uiNode in graph.Nodes)
            {
                NodeBase data = graph.GetElementData(uiNode.type);
                if (data == null) continue;

                int num = (uiNode.weight == int.MaxValue) ? 0 : uiNode.weight;
                float w = data.Weight > 0 ? data.Weight : 1.0f;

                var gn = calcGraph.CreateNode(data, num, w);
                map[uiNode.id] = gn;

                if (uiNode.type == ElementType.None)
                    calcGraph.StartNode = gn;
            }

            var processed = new System.Collections.Generic.HashSet<string>();
            foreach (var uiNode in graph.Nodes)
            {
                foreach (var targetId in uiNode.connectedIds)
                {
                    string key = string.Compare(uiNode.id, targetId) < 0
                        ? $"{uiNode.id}_{targetId}"
                        : $"{targetId}_{uiNode.id}";
                    if (!processed.Contains(key) && map.ContainsKey(uiNode.id) && map.ContainsKey(targetId))
                    {
                        calcGraph.Connect(map[uiNode.id], map[targetId]);
                        processed.Add(key);
                    }
                }
            }

            if (calcGraph.StartNode == null || calcGraph.Nodes.Count < 2)
            {
                compiledNode = null;
                return null;
            }

            calcGraph.CalculateWeight();
            compiledNode = SpellGraphSolver.SlowGraph(calcGraph);
            Debug.Log($"[SpellSlot {index}] скомпилирован: {compiledNode?.GetDominantAttack()} dmg={compiledNode?.Damage:F1}");
            return compiledNode;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SpellSlot {index}] ошибка компиляции: {ex.Message}");
            compiledNode = null;
            return null;
        }
    }
}