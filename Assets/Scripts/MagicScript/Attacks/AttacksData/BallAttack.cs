using UnityEngine;

public class BallAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        GameObject projectile = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        var spellScript = projectile.GetComponent<SpellProjectile>();

        spellScript.Setup(_nodeData.Damage, _nodeData.Range, _nodeData.Speed);
        
        ApplyVisual(projectile);
    }

    private void ApplyVisual(GameObject obj)
    {
        var renderer = obj.GetComponent<Renderer>();
    }

    public void Stop() { }
}