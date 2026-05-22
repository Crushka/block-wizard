using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

        CharacterController controller = null;
        Rigidbody rb = null;

        if (childPlayer != null)
        {
            controller = childPlayer.GetComponent<CharacterController>();
            rb = childPlayer.GetComponent<Rigidbody>();
        }

        if (controller != null)
        {
            controller.enabled = false;
        }
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

        if (controller != null)
        {
            controller.enabled = true;
        }
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Debug.Log($"[PlayerPersistence] Пустышка перемещена. Дочерний игрок '{childPlayer?.name}' выровнен по центру.");
    }
}