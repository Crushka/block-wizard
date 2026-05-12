using System.Collections;
using UnityEngine;

public class WaveAttackRunner : MonoBehaviour
{
    public void Run(Transform spawnPoint, NodeBase nodeData, float raycastHeightOffset,
                    GameObject prefab, LayerMask groundMask)
    {
        Vector3 spawnWorldPos = spawnPoint.position;
        Vector3? groundPos = GetGroundPoint(spawnWorldPos, raycastHeightOffset, groundMask);

        Vector3 waveOrigin = groundPos ?? spawnWorldPos;

        GameObject wave = Instantiate(prefab, waveOrigin, Quaternion.Euler(90f, 0f, 0f));
        var proj = wave.GetComponent<CircleWaveProjectile>();
        proj?.Setup(
            nodeData?.Damage ?? 15f,
            nodeData?.Range ?? 12f,
            nodeData?.Speed ?? 6f
        );

        Destroy(gameObject, 0.1f);
    }

    private static Vector3? GetGroundPoint(Vector3 worldPos, float heightOffset, LayerMask groundMask)
    {
        Vector3 rayOrigin = worldPos + Vector3.up * heightOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, heightOffset * 2f, groundMask))
            return hit.point;
        return null;
    }
}

public class CircleWaveProjectile : MonoBehaviour
{
    [Header("Кривые анимации")]
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private AnimationCurve fadeCurve;

    private float _damage;
    private float _range;
    private float _speed;

    private float _elapsedTime;
    private bool _isDestroyed;

    private Vector3 _startPos;

    private Material _materialInstance;
    private Color _originalColor;

    public void Setup(float damage, float range, float speed)
    {
        _damage = damage;
        _range = range > 5f ? 5f : range;
        _speed = speed;
        _elapsedTime = 0f;
        _isDestroyed = false;
        _startPos = transform.position;

        transform.localScale = Vector3.zero;

        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            _materialInstance = mr.material;
            _originalColor = _materialInstance.color;
        }
    }

    void Update()
    {
        if (_isDestroyed) return;

        _elapsedTime += Time.deltaTime;

        float duration = _range / _speed;
        float normalizedTime = Mathf.Clamp01(_elapsedTime / duration);

        float currentRadius = _range * normalizedTime;
        float visualScale = currentRadius * 2f;
        transform.localScale = new Vector3(visualScale, visualScale, 1f);

        if (heightCurve != null)
        {
            float heightOffset = heightCurve.Evaluate(normalizedTime);
            transform.position = _startPos + Vector3.up * heightOffset;
        }

        if (_materialInstance != null && fadeCurve != null)
        {
            float alpha = fadeCurve.Evaluate(normalizedTime);
            _materialInstance.color = new Color(
                _originalColor.r, _originalColor.g, _originalColor.b, alpha);
        }

        if (normalizedTime >= 1f)
            Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDestroyed) return;
        if (!other.CompareTag("Enemy")) return;

        Debug.Log($"[CircleWave] Hit {other.name} for {_damage} damage");
    }

    private void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_materialInstance != null)
            Destroy(_materialInstance);
    }
}