using UnityEngine;

public class ThunderAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;

    private const float CylinderRadius = 1.5f;
    private const float DefaultRange = 15f;
    private const float DamageInterval = 0.25f;

    private static readonly LayerMask EnemyMask = LayerMask.GetMask("Enemy");

    private GameObject _activeBolt;
    private ThunderProjectile _boltScript;
    private bool _isCasting;
    private float _damageTimer;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_activeBolt == null)
            CreateBolt(spawnPoint);

        float range = _nodeData?.Range ?? DefaultRange;
        Vector3 targetPos = ThunderTargetFinder.Find(
            spawnPoint.position,
            spawnPoint.forward,
            range,
            CylinderRadius,
            EnemyMask
            );
        _boltScript?.UpdateBolt(spawnPoint.position, targetPos);

        _damageTimer -= Time.deltaTime;
        if(_damageTimer <= 0)
        {
            _damageTimer = DamageInterval;

            Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.5f, EnemyMask);
            foreach(var enemy in hitEnemies)
            {
                IDamageable damageable = enemy.GetComponent<IDamageable>();
                if(damageable != null)
                {
                    damageable.takeDamage(_nodeData?.Damage ?? 10f);
                }
            }
        }
    }

    
    public void Stop()
    {
        _isCasting = false;
        _damageTimer = 0f;

        if (_activeBolt != null)
        {
            Object.Destroy(_activeBolt);
            _activeBolt = null;
            _boltScript = null;
        }
    }

    private void CreateBolt(Transform spawnPoint)
    {
        _activeBolt = Object.Instantiate(_prefab, spawnPoint.position, Quaternion.identity);
        _boltScript = _activeBolt.GetComponent<ThunderProjectile>();

        if (_boltScript == null)
        {
            Debug.LogError("[ThunderAttack] префаб не найден ThunderProjectile");
            Object.Destroy(_activeBolt);
            _activeBolt = null;
            return;
        }

        _boltScript.SetupVisual(_nodeData);

        _isCasting = true;
        _damageTimer = 0f;
    }

    float IAttack.getDamage()
    {
        return _nodeData?.Damage ?? 10f;
    }
}