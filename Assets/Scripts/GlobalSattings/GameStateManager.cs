using System.Collections.Generic;
using UnityEngine;
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public float PlayerHP = 100f;
    public float PlayerMaxHP = 100f;
    public string LastSpawnPointId { get; set; } = "default";

    public List<ElementType> SavedInventory = new();

    public List<SpellSlot> SpellSlots = new();
    public int ActiveSpellSlotIndex { get; set; } = 0;

    public List<string> DeadBossIds = new();

    public GraphModel SavedGraph
    {
        get => SpellSlots.Count > ActiveSpellSlotIndex ? SpellSlots[ActiveSpellSlotIndex].graph : null;
        set { EnsureSlot(ActiveSpellSlotIndex); SpellSlots[ActiveSpellSlotIndex].graph = value; }
    }

    [System.NonSerialized]
    private NodeBase _legacySpell;
    public NodeBase ResolvedSpell
    {
        get => SpellSlots.Count > ActiveSpellSlotIndex ? SpellSlots[ActiveSpellSlotIndex].compiledNode : _legacySpell;
        set { EnsureSlot(ActiveSpellSlotIndex); SpellSlots[ActiveSpellSlotIndex].compiledNode = value; _legacySpell = value; }
    }

    public bool HasSavedState => SpellSlots.Count > 0 && !SpellSlots[0].IsEmpty;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void EnsureSlot(int idx)
    {
        while (SpellSlots.Count <= idx) SpellSlots.Add(new SpellSlot(SpellSlots.Count));
    }

    public void SaveInventory(InventoryManager inv)
    {
        if (inv == null) return;
        SavedInventory.Clear();
        foreach (Transform child in inv.inventoryContainer)
        {
            var view = child.GetComponent<NodeView>();
            if (view?.Data != null && view.Data.type != ElementType.None)
                SavedInventory.Add(view.Data.type);
        }
        Debug.Log($"[GSM] Инвентарь сохранён: {SavedInventory.Count}");
    }

    public void SaveGraph()
    {
        EnsureSlot(ActiveSpellSlotIndex);
        SpellSlots[ActiveSpellSlotIndex].SnapshotFromEditor(NodeEditorManager.Instance);
    }

    public void SaveSpell(NodeBase spell)
    {
        EnsureSlot(ActiveSpellSlotIndex);
        SpellSlots[ActiveSpellSlotIndex].compiledNode = spell;
    }

    public void SaveAllSpellSlotsState()
    {
        if (SpellSlotManager.Instance != null)
        {
            SpellSlotManager.Instance.SaveCurrentSlot();
        }
        else if (NodeEditorManager.Instance != null)
        {
            EnsureSlot(ActiveSpellSlotIndex);
            SpellSlots[ActiveSpellSlotIndex].SnapshotFromEditor(NodeEditorManager.Instance);
            SpellSlots[ActiveSpellSlotIndex].Compile();
        }

        Debug.Log($"[GSM] Все слоты заклинаний ({SpellSlots.Count}) успешно сохранены перед переходом сцены.");
    }

    public void SaveHP(float hp) => PlayerHP = hp;

    public void RestoreInventory(InventoryManager inv)
    {
        if (inv == null) return;
        foreach (Transform child in inv.inventoryContainer) Object.Destroy(child.gameObject);

        if (SavedInventory.Count > 0)
        {
            foreach (var type in SavedInventory) inv.CreateNode(type);
            Debug.Log($"[GSM] Инвентарь восстановлен: {SavedInventory.Count}");
        }
        else
        {
            inv.InitInventory();
            Debug.Log("[GSM] Инвентарь: стандартный набор");
        }
    }

    public void RestoreGraph()
    {
        if (SpellSlots.Count == 0 || ActiveSpellSlotIndex >= SpellSlots.Count) return;
        SpellSlots[ActiveSpellSlotIndex].RestoreToEditor(NodeEditorManager.Instance);
    }

    public void RestoreSpell(SpellCaster caster)
    {
        if (caster == null || SpellSlots.Count == 0) return;

        var slot = SpellSlots[ActiveSpellSlotIndex];

        if (slot.compiledNode == null && !slot.IsEmpty)
            slot.Compile();

        if (slot.compiledNode != null)
            caster.PrepareSpellFromNode(slot.compiledNode);
    }

    public bool IsBossDead(string bossId) => DeadBossIds.Contains(bossId);

    public void RegisterBossDeath(string bossId)
    {
        if (!DeadBossIds.Contains(bossId))
        {
            DeadBossIds.Add(bossId);
            Debug.Log($"[GSM] Босс '{bossId}' зарегистрирован как мёртвый.");
        }
    }
}