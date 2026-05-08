using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    public Transform target;
    public float lookRadius = 10f;

    private NavMeshAgent agent;
    private int enemyLayerMask;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        enemyLayerMask = ~(1 << LayerMask.NameToLayer("Enemy"));

        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);
        }

        HandleRotation(distance <= lookRadius);
    }

    private void HandleRotation(bool isChasing)
    {
        RaycastHit hit;
        Vector3 surfaceNormal = Vector3.up;

        if (Physics.Raycast(transform.position + Vector3.up * 1.0f, Vector3.down, out hit, 3f, enemyLayerMask))
        {
            surfaceNormal = hit.normal;
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        Quaternion targetRotation;

        if (isChasing)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Vector3 forwardOnSlope = Vector3.ProjectOnPlane(directionToTarget, surfaceNormal);
            targetRotation = Quaternion.LookRotation(forwardOnSlope, surfaceNormal);
        }
        else
        {
            Vector3 forwardOnSlope = Vector3.ProjectOnPlane(transform.forward, surfaceNormal);
            targetRotation = Quaternion.LookRotation(forwardOnSlope, surfaceNormal);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}