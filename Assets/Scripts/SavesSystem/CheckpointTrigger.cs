using UnityEngine;

/// <summary>
/// Положи этот компонент на объект с коллайдером (Is Trigger = true).
/// При входе игрока — делает полное сохранение на диск.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("ID точки спавна, привязанной к этому чекпоинту (должна совпадать с SpawnPoint.spawnId)")]
    [SerializeField] private string spawnPointId = "checkpoint_1";

    [SerializeField] private string playerTag = "Player";

    [Header("Что сохранять")]
    [SerializeField] private bool saveInventory = true;
    [SerializeField] private bool saveSpellSlots = true;
    [SerializeField] private bool saveHP = true;

    [Header("Визуал (опционально)")]
    [SerializeField] private GameObject activeVisual;   // напр. свет / партикли "активного" чекпоинта
    [SerializeField] private GameObject inactiveVisual; // серый вариант

    private bool _activated = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        SetVisual(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_activated) return;   // уже сохранялись здесь в этой сессии

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

        // 1. Обновляем спавн-точку
        gsm.LastSpawnPointId = spawnPointId;

        // 2. HP
        if (saveHP)
        {
            var health = playerObject.GetComponent<PlayerHealth>();
            if (health != null)
                gsm.SaveHP(health.health);
        }

        // 3. Инвентарь
        if (saveInventory)
        {
            var inv = Object.FindAnyObjectByType<InventoryManager>();
            if (inv != null)
                gsm.SaveInventory(inv);
        }

        // 4. Слоты заклинаний (включая текущий граф в редакторе)
        if (saveSpellSlots)
            gsm.SaveAllSpellSlotsState();

        // 5. Пишем на диск
        SaveSystem.Save(gsm);

        _activated = true;
        SetVisual(true);

        Debug.Log($"[CheckpointTrigger] ✓ Сохранено на чекпоинте '{spawnPointId}'");
    }

    private void SetVisual(bool isActive)
    {
        if (activeVisual != null)   activeVisual.SetActive(isActive);
        if (inactiveVisual != null) inactiveVisual.SetActive(!isActive);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = _activated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            $"Checkpoint\n'{spawnPointId}'");
    }
#endif
}
