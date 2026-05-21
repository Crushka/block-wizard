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

    void Start()
    {
        var gsm = GameStateManager.Instance;

        if (gsm == null)
        {
            Debug.Log("[SceneBootstrap] GameStateManager не найден. Создаем временный для этой сцены.");
            var go = new GameObject("GameStateManager_AutoCreated");
            gsm = go.AddComponent<GameStateManager>();

            gsm.LastSpawnPointId = "default";
        }

        if (inventoryManager != null)
            gsm.RestoreInventory(inventoryManager);

        if (nodeEditorManager != null)
            gsm.RestoreGraph();

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

        StartCoroutine(SpawnPlayer());
    }

    private IEnumerator SpawnPlayer()
    {
        yield return new WaitForEndOfFrame();

        var gsm = GameStateManager.Instance;
        if (gsm == null) yield break;

        var player = PlayerPersistence.Instance;
        if (player == null)
        {
            Debug.LogError("[SceneBootstrap] КРИТИЧЕСКАЯ ОШИБКА: Игрок с компонентом PlayerPersistence не найден в DontDestroyOnLoad!");
            yield break;
        }

        var allSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude);

        SpawnPoint target = null;

        Debug.Log($"[SceneBootstrap] Начинаем поиск точки спавна. Ищем целевой ID: '{gsm.LastSpawnPointId}'");

        foreach (var sp in allSpawnPoints)
        {
            if (sp.spawnId == gsm.LastSpawnPointId)
            {
                target = sp;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[SceneBootstrap] Точка '{gsm.LastSpawnPointId}' не найдена на сцене. Пробуем найти точку 'default'...");
            foreach (var sp in allSpawnPoints)
            {
                if (sp.spawnId == "default")
                {
                    target = sp;
                    break;
                }
            }
        }

        if (target == null)
        {
            Debug.LogError("[SceneBootstrap] На сцене нет ни нужной точки спавна, ни точки 'default'! Игрок остался на старом месте.");
            yield break;
        }

        Debug.Log($"[SceneBootstrap] ТОЧКА НАЙДЕНА! Телепортируем игрока на спавнпоинт: '{target.spawnId}' (Позиция: {target.transform.position})");
        player.TeleportTo(target.transform);

        Debug.Log("[SceneBootstrap] Телепортация игрока успешно завершена!");
    }
}