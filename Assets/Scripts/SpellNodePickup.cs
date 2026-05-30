using UnityEngine;

public class SpellNodePickup : MonoBehaviour
{


    [Header("Тип ноды для выдачи")]
    [Tooltip("Какой элемент добавится в инвентарь игрока")]
    public ElementType elementType = ElementType.Fire;

    [Header("Количество")]
    [Tooltip("Сколько нод выдать за одно касание")]
    [Min(1)]
    public int amount = 1;

    [Header("Поведение после подбора")]
    [Tooltip("Уничтожить объект после подбора")]
    public bool destroyOnPickup = true;

    [Tooltip("Если destroyOnPickup = false — скрыть объект (можно включить повторно)")]
    public bool hideOnPickup = false;

    [Tooltip("Можно ли подобрать повторно (только если destroyOnPickup = false)")]
    public bool singleUse = true;

    [Header("Визуальный эффект")]
    [Tooltip("GameObject-эффект, который заспавнится в момент подбора (можно оставить пустым)")]
    public GameObject pickupEffectPrefab;


    private bool _alreadyPickedUp = false;


    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyPickedUp) return;
        if (!other.CompareTag("Player")) return;

        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[SpellNodePickup] InventoryManager.Instance == null. " +
                             "Убедись что InventoryManager присутствует на сцене.");
            return;
        }

        for (int i = 0; i < amount; i++)
            inventory.CreateNode(elementType);

        Debug.Log($"[SpellNodePickup] Выдано {amount}x {elementType} → инвентарь игрока.");

        if (pickupEffectPrefab != null)
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        if (singleUse) _alreadyPickedUp = true;

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else if (hideOnPickup)
        {
            gameObject.SetActive(false);
        }
    }
    public void ResetPickup()
    {
        _alreadyPickedUp = false;
        gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = new Color(1f, 0.6f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
