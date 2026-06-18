using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] public string spawnId = "checkpoint_1";

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position,
                        transform.position + transform.forward * 1.5f);
    }
}