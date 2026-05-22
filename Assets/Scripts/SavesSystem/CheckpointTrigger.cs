using UnityEngine;


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

        gsm.LastSpawnPointId = spawnPointId;

        if (saveHP)
        {
            var health = playerObject.GetComponent<PlayerHealth>();
            if (health != null)
                gsm.SaveHP(health.health);
        }

        if (saveInventory)
        {
            var inv = Object.FindAnyObjectByType<InventoryManager>();
            if (inv != null)
                gsm.SaveInventory(inv);
        }

        if (saveSpellSlots)
            gsm.SaveAllSpellSlotsState();

        SaveSystem.Save(gsm);

        _activated = true;
        SetVisual(true);

        Debug.Log($"[CheckpointTrigger] Сохранено на чекпоинте '{spawnPointId}'");
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
