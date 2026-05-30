using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    public bool IsDead { get; set; } = false;

    void Awake()
    {
        if (Instance != null && Instance.gameObject == null)
            Instance = null;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[PlayerPersistence] Зарегистрирован: {gameObject.name}");
            return;
        }

        if (Instance == this) return;

        Debug.Log("[PlayerPersistence] Дубликат уничтожен.");
        Destroy(gameObject);
    }

    public static void ClearInstance()
    {
        Instance = null;
    }

    public void TeleportTo(Transform targetTransform)
    {
        GameObject childPlayer = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                childPlayer = child.gameObject;
                break;
            }
        }

        CharacterController controller = childPlayer?.GetComponent<CharacterController>();
        Rigidbody rb = childPlayer?.GetComponent<Rigidbody>();

        if (controller != null) controller.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;

        if (childPlayer != null)
        {
            childPlayer.transform.localPosition = Vector3.zero;
            childPlayer.transform.localRotation = Quaternion.identity;
        }

        if (controller != null) controller.enabled = true;
        if (rb != null) rb.isKinematic = false;

        Debug.Log($"[PlayerPersistence] Телепортирован на {targetTransform.position}. Дочерний: '{childPlayer?.name}'");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}