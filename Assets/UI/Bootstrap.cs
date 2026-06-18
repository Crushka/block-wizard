using System.Collections;
using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    [Header("Ссылки сцены")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private NodeEditorManager nodeEditorManager;
    [SerializeField] private SpellCaster spellCaster;

    [Header("Восстанавливать заклинание?")]
    [SerializeField] private bool restoreSpell = true;

    [Header("Настройки спавна")]
    [Tooltip("Максимальное время ожидания появления игрока в сцене (сек)")]
    [SerializeField] private float playerWaitTimeout = 5f;

    void Start()
    {
        var gsm = GameStateManager.Instance;

        if (gsm == null)
        {
            Debug.Log("[SceneBootstrap] GameStateManager не найден. Создаём временный для этой сцены.");
            var go = new GameObject("GameStateManager_AutoCreated");
            gsm = go.AddComponent<GameStateManager>();
            gsm.LastSpawnPointId = "default";
        }

        // 1. Восстанавливаем инвентарь
        if (inventoryManager != null)
            gsm.RestoreInventory(inventoryManager);
        Debug.Log("[SceneBootstrap] восстановили инвентарь");
        // 2. Инициализируем слоты заклинаний
        var slotManager = FindAnyObjectByType<SpellSlotManager>();
        if (slotManager != null)
        {
            var book = FindAnyObjectByType<BookInteraction>();
            slotManager.LateInit(nodeEditorManager, spellCaster, book);
        }
        Debug.Log("[SceneBootstrap] инициализировали слоты заклинаний");

        // 3. Восстанавливаем граф заклинания в редакторе
        if (nodeEditorManager != null)
            gsm.RestoreGraph();
        Debug.Log("[SceneBootstrap] восстановили граф");

        // 4. Восстанавливаем активное заклинание
        if (restoreSpell && spellCaster != null)
        {
            try
            {
                gsm.RestoreSpell(spellCaster);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SceneBootstrap] Ошибка сборки заклинания: {ex.Message}");
            }
        }
        Debug.Log("[SceneBootstrap] восстановили активное заклинание");

        // 5. Запускаем поиск спавн-поинта на сцене по сохраненному ID и телепортируем игрока
        Debug.Log("[SceneBootstrap] запускаем телевортацию");
        StartCoroutine(SpawnPlayerAtSpawnPoint());
    }

    private IEnumerator SpawnPlayerAtSpawnPoint()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null) yield break;

        // Находим точку спавна по сохраненному ID (например, "checkpoint_1")
        SpawnPoint target = FindSpawnPoint(gsm.LastSpawnPointId);

        if (target == null)
        {
            Debug.LogError($"[SceneBootstrap] На сцене не найдена точка спавна '{gsm.LastSpawnPointId}' и нет точки 'default'!");
            yield break;
        }

        float waited = 0f;
        PlayerPersistence player = PlayerPersistence.Instance;

        // Ждем, пока сцена полностью проинициализирует игрока
        while (player == null && waited < playerWaitTimeout)
        {
            yield return null;
            waited += Time.unscaledDeltaTime;
            player = PlayerPersistence.Instance;
        }

        if (player == null)
        {
            Debug.LogError($"[SceneBootstrap] Игрок не появился за {playerWaitTimeout} сек. Телепортация отменена.");
            yield break;
        }

        yield return null; // Ждем один кадр для стабильности физики Unity

        // =====================================================================
        // ИСПРАВЛЕНИЕ БАГА С РАССИНХРОНОМ CHARACTER CONTROLLER
        // =====================================================================

        // 1. Ищем компонент CharacterController на игроке (или его дочерних объектах)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null) cc = player.GetComponentInChildren<CharacterController>();

        // 2. Перед телепортацией временно отключаем физику контроллера
        if (cc != null)
        {
            cc.enabled = false;
        }

        // 3. Телепортируем игрока на найденную точку через ваш метод
        player.TeleportTo(target.transform);

        // 4. Насильно просим физический движок Unity обновить координаты в текущем кадре
        Physics.SyncTransforms();

        // 5. Включаем физику обратно (теперь коллайдер встанет ровно на место визуала)
        if (cc != null)
        {
            cc.enabled = true;
        }

        // =====================================================================

        Debug.Log($"[SceneBootstrap] Игрок телепортирован на точку '{target.spawnId}' (координаты: {target.transform.position})");
    }

    // Метод поиска точки спавна на сцене по её ID
    private SpawnPoint FindSpawnPoint(string id)
    {
        var allPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude);

        // Сначала пытаемся найти точное совпадение
        foreach (var sp in allPoints)
        {
            if (sp.spawnId == id)
                return sp;
        }

        // Если точное совпадение не найдено (например, новая игра), ищем точку по умолчанию
        Debug.LogWarning($"[SceneBootstrap] Точка спавна '{id}' не найдена, ищем резервную 'default'");
        foreach (var sp in allPoints)
        {
            if (sp.spawnId == "default")
                return sp;
        }

        return null;
    }
}