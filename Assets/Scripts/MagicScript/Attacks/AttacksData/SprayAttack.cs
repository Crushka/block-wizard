using UnityEngine;

public class SprayAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;
    private GameObject _activeVFX;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_activeVFX != null) return;
        _activeVFX = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        _activeVFX.transform.SetParent(spawnPoint);
    }

    public void Stop()
    {
        if (_activeVFX == null) return;
        Object.Destroy(_activeVFX);
        _activeVFX = null;
    }
}