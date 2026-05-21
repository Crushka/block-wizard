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
        float range = nodeData?.Range ?? 12f;
        float damage = nodeData?.Damage ?? 15f;
        float speed = nodeData?.Speed ?? 6f;

        // 1. Фиксируем направление в момент каста
        Vector3 dir = spawnPoint.forward;
        dir.y = 0f;
        dir.Normalize();

        // 2. КРИТИЧЕСКИЙ ШАГ: Фиксируем стартовую позицию игрока в переменную!
        Vector3 startingOrigin = spawnPoint.position;

        // Корректируем стартовую точку по земле (если нужно)
        Vector3 origin = GetGroundPoint(startingOrigin, dir, 0f, raycastHeightOffset, groundMask) ?? startingOrigin;

        float distance = spacing;
        while (distance <= range)
        {
            // 3. Считаем позицию от ЗАФИКСИРОВАННОЙ стартовой точки, а не от живого spawnPoint
            Vector3 worldPos = startingOrigin + dir * distance;
            Vector3? groundPos = GetGroundPoint(worldPos, dir, distance, raycastHeightOffset, groundMask);

            if (groundPos.HasValue)
            {
                GameObject spike = Instantiate(prefab, groundPos.Value, Quaternion.identity);
                var proj = spike.GetComponent<SpikeProjectile>();
                proj?.Setup(damage, speed);
                proj?.SetupVisual(nodeData);
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

    public void SetupVisual(NodeBase node)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    float originalAlpha = mat.GetColor("_BaseColor").a;
                    Color c = node.PrimaryColor;
                    c.a = originalAlpha;
                    mat.SetColor("_BaseColor", c);
                }

                if (mat.HasProperty("_Color"))
                {
                    float originalAlpha = mat.GetColor("_Color").a;
                    Color c = node.PrimaryColor;
                    c.a = originalAlpha;
                    mat.SetColor("_Color", c);
                }

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", node.PrimaryColor * node.EmissionIntensity);
                }
            }
        }
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
