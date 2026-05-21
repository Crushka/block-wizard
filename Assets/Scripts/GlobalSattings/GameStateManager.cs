using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // ── Состояние игрока ──────────────────────────────────────────────────
    public float PlayerHP = 100f;
    public Vector3 PlayerPosition;     // опционально — часто сбрасывается на spawn

    // ── Инвентарь (ноды) ─────────────────────────────────────────────────
    // Сохраняем как список ElementType, потому что NodeModel не сериализуем
    public List<ElementType> InventoryNodes = new();

    // ── Заклинание (граф) ─────────────────────────────────────────────────
    // GraphModel уже помечен [Serializable] — сохраняем напрямую
    public GraphModel SavedGraph;
    public NodeBase ResolvedSpell;    // итог последнего компила

    public string LastSpawnPointId = "default";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Сохранение ────────────────────────────────────────────────────────

    public void SaveInventory(InventoryManager inv)
    {
        InventoryNodes.Clear();
        foreach (Transform child in inv.inventoryContainer)
        {
            var view = child.GetComponent<NodeView>();
            if (view != null && view.Data.type != ElementType.None)
                InventoryNodes.Add(view.Data.type);
        }
    }

    public void SaveGraph()
    {
        var mgr = NodeEditorManager.Instance;
        if (mgr == null) return;

        // Глубокое копирование, чтобы не терять данные при уничтожении сцены
        SavedGraph = new GraphModel();
        foreach (var node in mgr.Graph.Nodes)
        {
            var copy = new NodeModel(node.type) { id = node.id, weight = node.weight };
            foreach (var id in node.connectedIds) copy.AddLink(id);
            SavedGraph.Nodes.Add(copy);
        }
    }

    public void SaveSpell(NodeBase spell) => ResolvedSpell = spell;

    // ── Восстановление ────────────────────────────────────────────────────

    public void RestoreInventory(InventoryManager inv)
    {
        inv.InitInventory();   // очищает и создаёт новые ноды — можно заменить
        // Или так (если хочешь точное восстановление):
        // foreach (Transform child in inv.inventoryContainer) Destroy(child.gameObject);
        // foreach (var type in InventoryNodes) inv.CreateNode(type);
    }

    public void RestoreGraph()
    {
        var mgr = NodeEditorManager.Instance;
        if (mgr == null || SavedGraph == null) return;

        // Сначала создаём стартовый узел (он всегда нужен визуально)
        mgr.InitEditor();   // создаёт None-нод

        // Заменяем None-узел из InitEditor на тот что был сохранён
        var savedNone = SavedGraph.Nodes.Find(n => n.type == ElementType.None);
        if (savedNone != null)
        {
            // Обновляем id в менеджере чтобы связи работали
            var liveNone = mgr.Graph.Nodes.Find(n => n.type == ElementType.None);
            if (liveNone != null)
                liveNone.id = savedNone.id;
        }

        // Добавляем остальные ноды
        foreach (var node in SavedGraph.Nodes)
        {
            if (node.type == ElementType.None) continue;
            mgr.Graph.Nodes.Add(node);

            // Создаём NodeView для каждой ноды
            var obj = Object.Instantiate(mgr.nodePrefab, mgr.graphContainer);
            var view = obj.GetComponent<NodeView>();
            view.Initialize(node);
            view.SetVisualState(true);
        }

        mgr.RefreshGraph();
    }

    public void RestoreSpell(SpellCaster caster)
    {
        if (caster == null || ResolvedSpell == null) return;
        caster.PrepareSpellFromNode(ResolvedSpell);
    }
}