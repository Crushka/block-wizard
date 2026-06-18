//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;
//using SaveSystem.Data;
//public class InventorySlotManager : MonoBehaviour
//{
//    public static InventorySlotManager Instance { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//        Instance = this;
//    }

//    [Header("Inventory Slots")]
//    [SerializeField] private List<InventorySlot> inventorySlots = new();

//    [Header("Quick Slots — панель инвентаря (4 шт, порядок 0-3)")]
//    [SerializeField] private List<InventorySlot> quickSlots = new();

//    [Header("HUD Mirror Slots — UI игрока (тот же порядок 0-3, с HUDSlotActiveIndicator)")]
//    [SerializeField] private List<InventorySlot> hudMirrorSlots = new();

//    [Header("Prefab")]
//    [SerializeField] private GameObject inventoryItemPrefab;

//    [Header("Item Icons")]
//    [SerializeField] private List<ItemIconEntry> itemIcons = new();

//    [Header("Item Prefabs (для системы сохранения)")]


//    private readonly Dictionary<string, InventoryItem> _stackMap = new();

//    // ── Public API ────────────────────────────────────────────────────────────

//    public void AddItem(Item item)
//    {
//        if (item == null) { Debug.LogError("[Inventory] AddItem: item == null"); return; }

//        // Копируем данные сразу — объект будет уничтожен в этом же кадре
//        string itemId = item.id;
//        int itemAmount = item.amount;
//        Sprite icon = GetIcon(itemId);

//        Debug.Log($"[Inventory] AddItem called: id='{itemId}', amount={itemAmount}");

//        if (string.IsNullOrEmpty(itemId))
//        {
//            Debug.LogError("[Inventory] Item id пустой! Назначь id в инспекторе prefab-а предмета на сцене.");
//            return;
//        }

//        // Уже есть в инвентаре — добавляем к стаку
//        if (_stackMap.TryGetValue(itemId, out InventoryItem existing))
//        {
//            Debug.Log($"[Inventory] Стак найден для '{itemId}', добавляем {itemAmount}. Было: {existing.Count}");
//            existing.AddCount(itemAmount);
//            InventorySlot existingSlot = FindSlotWith(existing);
//            if (existingSlot != null && existingSlot.isQuickSlot)
//                existingSlot.SyncHUDMirror(existing);
//            return;
//        }

//        // Диагностика слотов
//        Debug.Log($"[Inventory] Свободные inventorySlots: {inventorySlots.Count(s => SlotIsEmpty(s))}/{inventorySlots.Count}");
//        Debug.Log($"[Inventory] Свободные quickSlots:     {quickSlots.Count(s => SlotIsEmpty(s))}/{quickSlots.Count}");

//        for (int i = 0; i < inventorySlots.Count; i++)
//        {
//            var s = inventorySlots[i];
//            var ci = s.CurrentItem;
//            bool unityNull = !(ci as UnityEngine.Object);
//            Debug.Log($"[Inventory]   inventorySlots[{i}]: CurrentItem={ci}, unityNull={unityNull}, isEmpty={SlotIsEmpty(s)}");
//        }

//        InventorySlot freeSlot =
//            inventorySlots.FirstOrDefault(s => SlotIsEmpty(s)) ??
//            quickSlots.FirstOrDefault(s => SlotIsEmpty(s));

//        if (freeSlot == null)
//        {
//            Debug.LogError($"[Inventory] Нет свободного слота для '{itemId}'! Все слоты заняты.");
//            return;
//        }

//        Debug.Log($"[Inventory] Кладём '{itemId}' в слот '{freeSlot.gameObject.name}'");
//        InventoryItem entry = CreateVisual(itemId, itemAmount, icon, item);
//        freeSlot.PlaceItem(entry);
//        _stackMap[itemId] = entry;
//        Debug.Log($"[Inventory] '{itemId}' успешно добавлен. _stackMap.Count={_stackMap.Count}");
//    }

//    public void CreateNode(ElementType nodeType)
//    {
//        Debug.Log($"[Inventory] CreateNode: {nodeType}");
//    }

//    public void ConsumeItem(string itemId, int amount = 1)
//    {
//        if (!_stackMap.TryGetValue(itemId, out InventoryItem entry)) return;
//        entry.AddCount(-amount);
//        if (entry.Count > 0) return;
//        _stackMap.Remove(itemId);
//        FindSlotWith(entry)?.ClearSlot(destroy: true);
//    }

//    public void ConsumeFromQuickSlot(int index)
//    {
//        if (index < 0 || index >= quickSlots.Count) return;

//        InventorySlot slot = quickSlots[index];
//        InventoryItem item = slot.CurrentItem;
//        if (item == null) return;

//        string itemId = item.ItemData?.id;
//        item.AddCount(-1);
//        Debug.Log($"[Inventory] ConsumeFromQuickSlot[{index}]: id='{itemId}', осталось={item.Count}");

//        if (item.Count <= 0)
//        {
//            if (!string.IsNullOrEmpty(itemId))
//                _stackMap.Remove(itemId);
//            slot.ClearSlot(destroy: true);
//        }
//        else
//        {
//            slot.SyncHUDMirror(item);
//        }
//    }

//    public InventoryItem GetQuickSlotItem(int index)
//    {
//        if (index < 0 || index >= quickSlots.Count) return null;
//        return quickSlots[index].CurrentItem;
//    }

//    public InventorySlot GetHUDMirror(int index)
//    {
//        if (index < 0 || index >= hudMirrorSlots.Count) return null;
//        return hudMirrorSlots[index];
//    }

//    public InventoryItem CloneVisual(InventoryItem source)
//    {
//        return CreateVisual(source.ItemData.id, source.Count, GetIcon(source.ItemData.id), source.ItemData);
//    }

//    // ── Private helpers ───────────────────────────────────────────────────────

//    private static bool SlotIsEmpty(InventorySlot slot)
//    {
//        InventoryItem ci = slot.CurrentItem;
//        if (ci == null) return true;
//        if (!(ci as UnityEngine.Object)) return true;
//        return false;
//    }

//    private InventoryItem CreateVisual(string itemId, int count, Sprite icon, Item sourceItem)
//    {
//        GameObject go = Instantiate(inventoryItemPrefab);
//        InventoryItem iv = go.GetComponent<InventoryItem>();
//        iv.Initialise(sourceItem, count, icon);
//        if (go.GetComponent<DraggableItem>() == null)
//            go.AddComponent<DraggableItem>();
//        return iv;
//    }

//    private Sprite GetIcon(string itemId)
//    {
//        foreach (var e in itemIcons)
//            if (e.id == itemId) return e.sprite;
//        return null;
//    }

//    private InventorySlot FindSlotWith(InventoryItem item)
//    {
//        foreach (var s in inventorySlots) if (s.CurrentItem == item) return s;
//        foreach (var s in quickSlots) if (s.CurrentItem == item) return s;
//        return null;
//    }

//    [System.Serializable]
//    public class ItemIconEntry
//    {
//        public string id;
//        public Sprite sprite;
//    }
//}


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SaveSystem.Data; // Подключаем пространство имен данных сохранения

public class InventorySlotManager : MonoBehaviour
{
    public static InventorySlotManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("Inventory Slots")]
    [SerializeField] private List<InventorySlot> inventorySlots = new();

    [Header("Quick Slots — панель инвентаря (4 шт, порядок 0-3)")]
    [SerializeField] private List<InventorySlot> quickSlots = new();

    [Header("HUD Mirror Slots — UI игрока (тот же порядок 0-3, с HUDSlotActiveIndicator)")]
    [SerializeField] private List<InventorySlot> hudMirrorSlots = new();

    [Header("Prefab")]
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Item Icons")]
    [SerializeField] private List<ItemIconEntry> itemIcons = new();

    // НОВОЕ: Список префабов предметов для их воссоздания при загрузке сохранения
    [Header("Item Prefabs (для системы сохранения)")]
    [SerializeField] private List<ItemPrefabEntry> itemPrefabs = new();

    private readonly Dictionary<string, InventoryItem> _stackMap = new();

    // ── Public API ────────────────────────────────────────────────────────────

    // НОВОЕ: Публичный доступ к слотам для считывания состояния менеджером сохранений
    public List<InventorySlot> GetInventorySlots() => inventorySlots;
    public List<InventorySlot> GetQuickSlots() => quickSlots;

    public void AddItem(Item item)
    {
        if (item == null) { Debug.LogError("[Inventory] AddItem: item == null"); return; }

        string itemId = item.id;
        int itemAmount = item.amount;
        Sprite icon = GetIcon(itemId);

        Debug.Log($"[Inventory] AddItem called: id='{itemId}', amount={itemAmount}");

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("[Inventory] Item id пустой! Назначь id в инспекторе prefab-а предмета на сцене.");
            return;
        }

        if (_stackMap.TryGetValue(itemId, out InventoryItem existing))
        {
            Debug.Log($"[Inventory] Стак найден для '{itemId}', добавляем {itemAmount}. Было: {existing.Count}");
            existing.AddCount(itemAmount);
            InventorySlot existingSlot = FindSlotWith(existing);
            if (existingSlot != null && existingSlot.isQuickSlot)
                existingSlot.SyncHUDMirror(existing);
            return;
        }

        InventorySlot freeSlot =
            inventorySlots.FirstOrDefault(s => SlotIsEmpty(s)) ??
            quickSlots.FirstOrDefault(s => SlotIsEmpty(s));

        if (freeSlot == null)
        {
            Debug.LogError($"[Inventory] Нет свободного слота для '{itemId}'! Все слоты заняты.");
            return;
        }

        Debug.Log($"[Inventory] Кладём '{itemId}' в слот '{freeSlot.gameObject.name}'");
        InventoryItem entry = CreateVisual(itemId, itemAmount, icon, item);
        freeSlot.PlaceItem(entry);
        _stackMap[itemId] = entry;
    }

    // НОВОЕ: Очистка всех слотов перед загрузкой состояния
    public void ClearAllSlots()
    {
        _stackMap.Clear();
        foreach (var slot in inventorySlots)
        {
            slot.ClearSlot(destroy: true);
        }
        foreach (var slot in quickSlots)
        {
            slot.ClearSlot(destroy: true);
        }
    }

    // НОВОЕ: Восстановление инвентаря из списка сохраненных данных
    public void LoadInventoryState(List<SavedItemSlot> savedSlots)
    {
        ClearAllSlots();

        if (savedSlots == null) return;

        foreach (var saved in savedSlots)
        {
            Sprite icon = GetIcon(saved.ItemId);
            Item prefab = GetPrefab(saved.ItemId);

            // Создаем визуальное представление, передавая в качестве источника префаб предмета
            InventoryItem entry = CreateVisual(saved.ItemId, saved.Count, icon, prefab);

            if (saved.IsQuickSlot)
            {
                if (saved.SlotIndex >= 0 && saved.SlotIndex < quickSlots.Count)
                {
                    quickSlots[saved.SlotIndex].PlaceItem(entry);
                }
            }
            else
            {
                if (saved.SlotIndex >= 0 && saved.SlotIndex < inventorySlots.Count)
                {
                    inventorySlots[saved.SlotIndex].PlaceItem(entry);
                }
            }

            // Добавляем обратно в карту стаков
            _stackMap[saved.ItemId] = entry;
        }
    }

    public void CreateNode(ElementType nodeType)
    {
        Debug.Log($"[Inventory] CreateNode: {nodeType}");
    }

    public void ConsumeItem(string itemId, int amount = 1)
    {
        if (!_stackMap.TryGetValue(itemId, out InventoryItem entry)) return;
        entry.AddCount(-amount);
        if (entry.Count > 0) return;
        _stackMap.Remove(itemId);
        FindSlotWith(entry)?.ClearSlot(destroy: true);
    }

    public void ConsumeFromQuickSlot(int index)
    {
        if (index < 0 || index >= quickSlots.Count) return;

        InventorySlot slot = quickSlots[index];
        InventoryItem item = slot.CurrentItem;
        if (item == null) return;

        string itemId = item.ItemData?.id;
        item.AddCount(-1);
        Debug.Log($"[Inventory] ConsumeFromQuickSlot[{index}]: id='{itemId}', осталось={item.Count}");

        if (item.Count <= 0)
        {
            if (!string.IsNullOrEmpty(itemId))
                _stackMap.Remove(itemId);
            slot.ClearSlot(destroy: true);
        }
        else
        {
            slot.SyncHUDMirror(item);
        }
    }

    public InventoryItem GetQuickSlotItem(int index)
    {
        if (index < 0 || index >= quickSlots.Count) return null;
        return quickSlots[index].CurrentItem;
    }

    public InventorySlot GetHUDMirror(int index)
    {
        if (index < 0 || index >= hudMirrorSlots.Count) return null;
        return hudMirrorSlots[index];
    }

    public InventoryItem CloneVisual(InventoryItem source)
    {
        Debug.Log("[InventorySlotManager]");
        Debug.Log("[InventorySlotManager]" + source.ItemData.id );
        Debug.Log("[InventorySlotManager]" + source.ItemData.id + source.Count + source.ItemData.id + source.ItemData);
        return CreateVisual(source.ItemData.id, source.Count, GetIcon(source.ItemData.id), source.ItemData);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool SlotIsEmpty(InventorySlot slot)
    {
        InventoryItem ci = slot.CurrentItem;
        if (ci == null) return true;
        if (!(ci as UnityEngine.Object)) return true;
        return false;
    }

    private InventoryItem CreateVisual(string itemId, int count, Sprite icon, Item sourceItem)
    {
        GameObject go = Instantiate(inventoryItemPrefab);
        InventoryItem iv = go.GetComponent<InventoryItem>();

        // ИСПРАВЛЕНИЕ БАГА: Объект сцены sourceItem уничтожается через Destroy(gameObject) при подборе.
        // Чтобы данные предмета (например, Potion) оставались доступны для активации, мы создаем
        // его долгоживущую копию на самом объекте UI-ячейки и выключаем ее активность на сцене.
        Item persistentItem = null;
        Item prefab = GetPrefab(itemId);

        if (prefab != null)
        {
            Item itemInstance = Instantiate(prefab, go.transform);
            itemInstance.gameObject.SetActive(false); // Выключаем физику и рендер, нам нужен только скрипт
            persistentItem = itemInstance;
        }
        else if (sourceItem != null)
        {
            Item itemInstance = Instantiate(sourceItem, go.transform);
            itemInstance.gameObject.SetActive(false);
            persistentItem = itemInstance;
        }

        iv.Initialise(persistentItem, count, icon);

        if (go.GetComponent<DraggableItem>() == null)
            go.AddComponent<DraggableItem>();
        return iv;
    }

    private Sprite GetIcon(string itemId)
    {
        foreach (var e in itemIcons)
            if (e.id == itemId) return e.sprite;
        return null;
    }

    private Item GetPrefab(string itemId)
    {
        foreach (var e in itemPrefabs)
            if (e.id == itemId) return e.prefab;
        return null;
    }

    private InventorySlot FindSlotWith(InventoryItem item)
    {
        foreach (var s in inventorySlots) if (s.CurrentItem == item) return s;
        foreach (var s in quickSlots) if (s.CurrentItem == item) return s;
        return null;
    }

    [System.Serializable]
    public class ItemIconEntry
    {
        public string id;
        public Sprite sprite;
    }

    // НОВОЕ: Класс соответствия id предмета его префабу
    [System.Serializable]
    public class ItemPrefabEntry
    {
        public string id;
        public Item prefab;
    }
}