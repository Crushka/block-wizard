using System.Collections;
using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TeleportTo(Transform spawnPoint)
    {
        StartCoroutine(ExecuteTeleport(spawnPoint));
    }

    private IEnumerator ExecuteTeleport(Transform spawnPoint)
    {
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        yield return new WaitForEndOfFrame();

        Vector3 targetPos = spawnPoint.position;
        Quaternion targetRot = spawnPoint.rotation;

        transform.SetPositionAndRotation(targetPos, targetRot);
        Physics.SyncTransforms();

        yield return null;

        transform.SetPositionAndRotation(targetPos, targetRot);
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
    }
}