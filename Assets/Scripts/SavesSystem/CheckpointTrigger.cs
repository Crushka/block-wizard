using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("ID точки спавна, привязанной к этому чекпоинту (должна совпадать с SpawnPoint.spawnId)")]
    [SerializeField] private string spawnPointId = "checkpoint_1";

    [SerializeField] private string playerTag = "Player";

    [Header("Что обновлять в памяти")]
    [SerializeField] private bool saveInventory = true;
    [SerializeField] private bool saveSpellSlots = true;
    [SerializeField] private bool saveHP = true;

    [Header("Визуал (опционально)")]
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private GameObject inactiveVisual;

    private bool _activated = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        SetVisual(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_activated) return;

        DoSave(other.gameObject);
    }

    private void DoSave(GameObject playerObject)
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[CheckpointTrigger] GameStateManager не найден!");
            return;
        }

        // 1. Обновляем нужные данные внутри Менеджера (в оперативной памяти)
        gsm.LastSpawnPointId = spawnPointId;

        if (saveHP)
        {
            // Убедись, что скрипт PlayerHealth существует на игроке
            var health = playerObject.GetComponent<PlayerHealth>();
            if (health != null)
                gsm.SaveHP(health.health);
        }

        if (saveInventory)
        {
            var inv = FindAnyObjectByType<InventoryManager>(FindObjectsInactive.Include);
            Debug.Log($"[CheckpointTrigger] InventoryManager найден: {inv != null}");
            if (inv != null)
            {
                Debug.Log($"[CheckpointTrigger] inventoryContainer: {inv.inventoryContainer != null}");
                gsm.SaveInventory(inv);
            }
        }

        if (saveSpellSlots)
        {
            gsm.SaveAllSpellSlotsState();
        }

        // 2. ФИНАЛЬНЫЙ АККОРД: Скидываем всю собранную в GSM информацию на жесткий диск!
        gsm.CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        gsm.SaveGameToDisk();

        // 3. Визуальная часть триггера
        _activated = true;
        SetVisual(true);

        Debug.Log($"[CheckpointTrigger] Сохранено на диск с чекпоинта '{spawnPointId}'");
    }

    private void SetVisual(bool isActive)
    {
        if (activeVisual != null) activeVisual.SetActive(isActive);
        if (inactiveVisual != null) inactiveVisual.SetActive(!isActive);
    }
}