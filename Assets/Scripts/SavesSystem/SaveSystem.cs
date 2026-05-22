using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Контейнер сохранения — всё что нужно пережить между сессиями
// ─────────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class SaveData
{
    // ── Позиция / сцена ──────────────────────────────────────────────────────
    public string sceneName;
    public string spawnPointId;

    // ── Здоровье ─────────────────────────────────────────────────────────────
    public float playerHP;
    public float playerMaxHP;

    // ── Инвентарь нод ────────────────────────────────────────────────────────
    public List<int> inventoryElementTypes = new();   // ElementType as int

    // ── Слоты заклинаний ─────────────────────────────────────────────────────
    public List<SpellSlotData> spellSlots = new();
    public int activeSpellSlotIndex;

    // ── Мёртвые боссы ────────────────────────────────────────────────────────
    public List<string> deadBossIds = new();
}

[System.Serializable]
public class SpellSlotData
{
    public int index;
    public List<NodeModelData> nodes = new();
}

[System.Serializable]
public class NodeModelData
{
    public string id;
    public int elementType;   // ElementType as int
    public int weight;
    public float posX;
    public float posY;
    public List<string> connectedIds = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// SaveSystem — читает / пишет SaveData на диск (JSON)
// ─────────────────────────────────────────────────────────────────────────────
public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    // ── Сохранение ────────────────────────────────────────────────────────────
    public static void Save(GameStateManager gsm)
    {
        if (gsm == null) { Debug.LogWarning("[SaveSystem] GSM == null"); return; }

        var data = new SaveData
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            spawnPointId = gsm.LastSpawnPointId,
            playerHP = gsm.PlayerHP,
            playerMaxHP = gsm.PlayerMaxHP,
            activeSpellSlotIndex = gsm.ActiveSpellSlotIndex,
            deadBossIds = new List<string>(gsm.DeadBossIds),
        };

        // Инвентарь
        foreach (var t in gsm.SavedInventory)
            data.inventoryElementTypes.Add((int)t);

        // Слоты заклинаний
        foreach (var slot in gsm.SpellSlots)
        {
            var slotData = new SpellSlotData { index = slot.index };

            if (slot.graph != null)
            {
                foreach (var node in slot.graph.Nodes)
                {
                    var nd = new NodeModelData
                    {
                        id          = node.id,
                        elementType = (int)node.type,
                        weight      = node.weight,
                        posX        = node.anchoredPosition.x,
                        posY        = node.anchoredPosition.y,
                    };
                    nd.connectedIds.AddRange(node.connectedIds);
                    slotData.nodes.Add(nd);
                }
            }

            data.spellSlots.Add(slotData);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Сохранено → {SavePath}");
    }

    // ── Загрузка ──────────────────────────────────────────────────────────────
    public static bool Load(GameStateManager gsm)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] Файл сохранения не найден.");
            return false;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data;

        try { data = JsonUtility.FromJson<SaveData>(json); }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] Ошибка парсинга: {ex.Message}");
            return false;
        }

        // Позиция
        gsm.LastSpawnPointId     = data.spawnPointId;
        gsm.PlayerHP             = data.playerHP;
        gsm.PlayerMaxHP          = data.playerMaxHP;
        gsm.ActiveSpellSlotIndex = data.activeSpellSlotIndex;

        // Мёртвые боссы
        gsm.DeadBossIds.Clear();
        gsm.DeadBossIds.AddRange(data.deadBossIds);

        // Инвентарь
        gsm.SavedInventory.Clear();
        foreach (var t in data.inventoryElementTypes)
            gsm.SavedInventory.Add((ElementType)t);

        // Слоты заклинаний
        gsm.SpellSlots.Clear();
        foreach (var slotData in data.spellSlots)
        {
            var slot = new SpellSlot(slotData.index);

            if (slotData.nodes.Count > 0)
            {
                slot.graph = new GraphModel();
                foreach (var nd in slotData.nodes)
                {
                    var node = new NodeModel((ElementType)nd.elementType)
                    {
                        id                = nd.id,
                        weight            = nd.weight,
                        anchoredPosition  = new Vector2(nd.posX, nd.posY),
                    };
                    node.connectedIds.AddRange(nd.connectedIds);
                    slot.graph.Nodes.Add(node);
                }
            }

            gsm.SpellSlots.Add(slot);
        }

        Debug.Log($"[SaveSystem] Загружено. Сцена: {data.sceneName}, спавн: {data.spawnPointId}");
        return true;
    }

    // ── Утилиты ───────────────────────────────────────────────────────────────
    public static bool HasSave() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
        Debug.Log("[SaveSystem] Сохранение удалено.");
    }

    /// <summary>Возвращает имя сцены из сохранения без загрузки в GSM (для главного меню).</summary>
    public static string PeekSavedScene()
    {
        if (!File.Exists(SavePath)) return null;
        try
        {
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            return data.sceneName;
        }
        catch { return null; }
    }
}
