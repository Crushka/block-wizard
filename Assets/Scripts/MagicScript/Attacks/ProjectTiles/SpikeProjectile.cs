using System.Collections;
using UnityEngine;

public class SpikeAttackRunner : MonoBehaviour
{
    public void Run(Transform spawnPoint, NodeBase nodeData, GameObject prefab,
                    float spacing, float spawnDelay,
                    float raycastHeightOffset, LayerMask groundMask)
    {
        StartCoroutine(SpawnSpikes(spawnPoint, nodeData, prefab,
                                   spacing, spawnDelay, raycastHeightOffset, groundMask));
    }

    private IEnumerator SpawnSpikes(Transform spawnPoint, NodeBase nodeData, GameObject prefab,
                                     float spacing, float spawnDelay,
                                     float raycastHeightOffset, LayerMask groundMask)
    {
        float range = nodeData?.Range  ?? 12f;
        float damage = nodeData?.Damage ?? 15f;
        float speed = nodeData?.Speed  ?? 6f;

        Vector3 dir = spawnPoint.forward;
        dir.y = 0f;
        dir.Normalize();

        Vector3 origin = spawnPoint.position;
        origin = GetGroundPoint(origin, dir, 0f, raycastHeightOffset, groundMask) ?? origin;

        float distance = spacing;
        while (distance <= range)
        {
            Vector3 worldPos = spawnPoint.position + dir * distance;
            Vector3? groundPos = GetGroundPoint(worldPos, dir, distance, raycastHeightOffset, groundMask);

            if (groundPos.HasValue)
            {
                GameObject spike = Instantiate(prefab, groundPos.Value, Quaternion.identity);
                var proj = spike.GetComponent<SpikeProjectile>();
                proj?.Setup(damage, speed);
            }

            distance += spacing;
            yield return new WaitForSeconds(spawnDelay);
        }

        Destroy(gameObject, 0.1f);
    }

    private static Vector3? GetGroundPoint(Vector3 worldPos, Vector3 dir, float dist,
                                            float heightOffset, LayerMask groundMask)
    {
        Vector3 rayOrigin = worldPos + Vector3.up * heightOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, heightOffset * 2f, groundMask))
            return hit.point;
        return null;
    }
}

public class SpikeProjectile : MonoBehaviour
{
    [Header("Визуал")]
    public float riseHeight = 1.5f;
    public float riseTime = 0.15f;
    public float holdTime = 0.4f;
    public float sinkTime = 0.2f;

    private float _damage;
    private float _speed; 
    private bool  _isDead;

    
    private Vector3 _belowGround;
    private Vector3 _peakPos;

    public void Setup(float damage, float speed)
    {
        _damage = damage;
        _speed  = speed;
    }

    private void Start()
    {
        _belowGround = transform.position;
        _peakPos = _belowGround + Vector3.up * riseHeight;

        transform.position = _belowGround + Vector3.down * riseHeight;

        StartCoroutine(SpikeLifecycle());
    }

    private IEnumerator SpikeLifecycle()
    {
        yield return MoveTowards(_belowGround + Vector3.down * riseHeight, _peakPos, riseTime);

        yield return new WaitForSeconds(holdTime);

        yield return MoveTowards(_peakPos, _belowGround + Vector3.down * riseHeight, sinkTime);

        Destroy(gameObject);
    }

    private IEnumerator MoveTowards(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.position = to;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;
        if (other.CompareTag("Player") || other.CompareTag("SprayProjectile")) return;

        
    }
}
