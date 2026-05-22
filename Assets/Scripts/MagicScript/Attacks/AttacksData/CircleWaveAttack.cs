using UnityEngine;

public class CircleWaveAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;
    private bool _hasFired;

    private const float RaycastHeightOffset = 10f;
    private static readonly LayerMask GroundMask = ~0;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_hasFired) return;
        _hasFired = true;

        var go = new GameObject("WaveAttackRunner");
        var runner = go.AddComponent<WaveAttackRunner>();
        runner.Run(spawnPoint, _nodeData, RaycastHeightOffset, _prefab, GroundMask);
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