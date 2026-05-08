using UnityEngine;

public class SprayAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;

    private const float FireInterval = 0.005f;
    private const float ProjectileSpeed = 12f;
    private const float ProjectileLifetime = 1.8f;
    private const float SpreadAngle = 12f;

    private float _fireTimer;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;
        _fireTimer = FireInterval;

        SpawnParticle(spawnPoint);
    }

    public void Stop()
    {
        _fireTimer = 0f;
    }

    private void SpawnParticle(Transform spawnPoint)
    {
        if (_prefab == null) return;

        GameObject proj = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        proj.tag = "SprayProjectile";

       

        var spray = proj.GetComponent<SprayProjectile>();
        if (spray != null)
        {
            float dmg = _nodeData != null ? _nodeData.Damage : 10f;
            spray.Setup(dmg, ProjectileSpeed, ProjectileLifetime, SpreadAngle);
        }
        else
        {
            var spell = proj.GetComponent<SpellProjectile>();
            spell?.Setup(_nodeData?.Damage ?? 10f, _nodeData?.Range ?? 12f, _nodeData?.Speed ?? 10f);
        }
    }
}