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

    [Header("Настройки ожидания игрока")]
    [Tooltip("Максимальное время ожидания появления игрока (сек)")]
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

        if (inventoryManager != null)
            gsm.RestoreInventory(inventoryManager);

        var slotManager = FindAnyObjectByType<SpellSlotManager>();
        if (slotManager != null)
        {
            var book = FindAnyObjectByType<BookInteraction>();
            slotManager.LateInit(nodeEditorManager, spellCaster, book);
        }

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
        var gsm = GameStateManager.Instance;
        if (gsm == null) yield break;

        SpawnPoint target = FindSpawnPoint(gsm.LastSpawnPointId);

        if (target == null)
        {
            Debug.LogError("[SceneBootstrap] На сцене нет нужной точки спавна и нет 'default'! Телепортация отменена.");
            yield break;
        }

        Debug.Log($"[SceneBootstrap] Точка спавна найдена: '{target.spawnId}' → {target.transform.position}");

        float waited = 0f;
        PlayerPersistence player = PlayerPersistence.Instance;

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

        Debug.Log($"[SceneBootstrap] Игрок найден (ждали {waited:F2}с). Телепортируем на '{target.spawnId}'.");

        yield return null;

        player.TeleportTo(target.transform);

        Debug.Log("[SceneBootstrap] Телепортация завершена!");
    }

    private SpawnPoint FindSpawnPoint(string id)
    {
        var allPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude);

        foreach (var sp in allPoints)
        {
            if (sp.spawnId == id)
            {
                Debug.Log($"[SceneBootstrap] Найдена точка спавна: '{id}'");
                return sp;
            }
        }

        Debug.LogWarning($"[SceneBootstrap] Точка '{id}' не найдена, ищем default");
        foreach (var sp in allPoints)
        {
            if (sp.spawnId == "default")
                return sp;
        }

        return null;
    }
}