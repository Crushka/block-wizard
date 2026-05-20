using UnityEngine;

public class BallAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;
    private bool _hasFired;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_hasFired) return;
        _hasFired = true;

        GameObject projectile = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        var spellScript = projectile.GetComponent<SpellProjectile>();
        spellScript.Setup(_nodeData.Damage, _nodeData.Range, _nodeData.Speed);
    }

    public void Stop()
    {
        _hasFired = false;
    }

    float IAttack.getDamage()
    {
        return _nodeData?.Damage ?? 10f;
    }
}