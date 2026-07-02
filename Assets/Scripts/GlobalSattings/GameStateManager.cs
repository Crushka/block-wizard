

using System.Collections.Generic;
using UnityEngine;
using SaveSystem.Data;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public string CurrentSceneName { get; set; } = "Level1";
    public float PlayerHP = 100f;
    public float PlayerMaxHP = 100f;
    public string LastSpawnPointId { get; set; } = "default";

    public List<ElementType> SavedInventory = new();
    public List<SavedItemSlot> SavedItemInventory = new();

    public List<SpellSlot> SpellSlots = new();
    public int ActiveSpellSlotIndex { get; set; } = 0;

    public List<string> DeadBossIds = new();

    private SaveSystem.SaveSystem _saveSystem;

    public bool HasSaveFile() => _saveSystem != null && _saveSystem.HasSave();
    public void DeleteSaveFile() => _saveSystem?.DeleteSave();

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

        string folder = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
        _saveSystem = new SaveSystem.SaveSystem(folder);

        LoadGameFromDisk();
    }

    public void SaveGameToDisk()
    {
        GameData data = new GameData();

        data.PlayerState.PlayerHP = PlayerHP;
        data.PlayerState.PlayerMaxHP = PlayerMaxHP;
        data.PlayerState.LastSpawnPointId = LastSpawnPointId;
        data.PlayerState.ActiveSpellSlotIndex = ActiveSpellSlotIndex;
        data.PlayerState.CurrentSceneName = this.CurrentSceneName;

        data.PlayerProgress.DeadBossIds = new List<string>(DeadBossIds);

        data.PlayerProgress.SavedInventory = new List<int>();
        foreach (var item in this.SavedInventory)
        {
            data.PlayerProgress.SavedInventory.Add((int)item);
        }

        var itemMgr = InventorySlotManager.Instance;
        if (itemMgr != null)
        {
            SavedItemInventory.Clear();

            var normalSlots = itemMgr.GetInventorySlots();
            for (int i = 0; i < normalSlots.Count; i++)
            {
                var slot = normalSlots[i];
                if (slot.CurrentItem != null && slot.CurrentItem.ItemData != null)
                {
                    SavedItemInventory.Add(new SavedItemSlot
                    {
                        ItemId = slot.CurrentItem.ItemData.id,
                        Count = slot.CurrentItem.Count,
                        SlotIndex = i,
                        IsQuickSlot = false
                    });
                }
            }

            var quickSlots = itemMgr.GetQuickSlots();
            for (int i = 0; i < quickSlots.Count; i++)
            {
                var slot = quickSlots[i];
                if (slot.CurrentItem != null && slot.CurrentItem.ItemData != null)
                {
                    SavedItemInventory.Add(new SavedItemSlot
                    {
                        ItemId = slot.CurrentItem.ItemData.id,
                        Count = slot.CurrentItem.Count,
                        SlotIndex = i,
                        IsQuickSlot = true
                    });
                }
            }
        }
        Debug.Log("[GSM] начало");

        data.PlayerProgress.SavedItemSlots = new List<SavedItemSlot>(this.SavedItemInventory);

        data.PlayerProgress.SpellSlots = new List<SpellSlot>(this.SpellSlots);

        _saveSystem.Save(data);
        Debug.Log("[GSM] ИГРА УСПЕШНО СОХРАНЕНА НА ДИСК!");
    }

    public void LoadGameFromDisk()
    {
        GameData data = _saveSystem.Load();

        PlayerHP = data.PlayerState.PlayerHP == 0 ? 100f : data.PlayerState.PlayerHP;
        PlayerMaxHP = data.PlayerState.PlayerMaxHP == 0 ? 100f : data.PlayerState.PlayerMaxHP;
        LastSpawnPointId = string.IsNullOrEmpty(data.PlayerState.LastSpawnPointId) ? "default" : data.PlayerState.LastSpawnPointId;
        ActiveSpellSlotIndex = data.PlayerState.ActiveSpellSlotIndex;

        DeadBossIds = data.PlayerProgress.DeadBossIds ?? new List<string>();

        SavedInventory.Clear();
        if (data.PlayerProgress.SavedInventory != null)
        {
            foreach (int itemInt in data.PlayerProgress.SavedInventory)
            {
                Debug.Log(itemInt);
                SavedInventory.Add((ElementType)itemInt);
            }
        }
        SavedItemInventory = data.PlayerProgress.SavedItemSlots ?? new List<SavedItemSlot>();


        CurrentSceneName = string.IsNullOrEmpty(data.PlayerState.CurrentSceneName) ? "Level1" : data.PlayerState.CurrentSceneName;
       
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
            Debug.Log($"[GSM] Нода в инвентаре: view={view != null}, type={view?.Data?.type}");
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
            SpellSlotManager.Instance.SaveAllSlots();
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

    public void RestoreItemInventory(InventorySlotManager itemMgr)
    {
        if (itemMgr == null) return;
        itemMgr.LoadInventoryState(SavedItemInventory);
        Debug.Log($"[GSM] Обычный инвентарь восстановлен: {SavedItemInventory.Count} предметов");
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