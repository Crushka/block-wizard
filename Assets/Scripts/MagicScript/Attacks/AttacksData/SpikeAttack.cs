using UnityEngine;

public class SpikeAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;
    private bool _hasFired;

    private const float Spacing = 1.5f;
    private const float SpawnDelay = 0.08f;
    private const float RaycastHeightOffset = 10f;
    private static readonly LayerMask GroundMask = ~0;

    private SpikeAttackRunner _runner;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_hasFired) return;
        _hasFired = true;

        var go = new GameObject("SpikeAttackRunner");
        _runner = go.AddComponent<SpikeAttackRunner>();
        _runner.Run(spawnPoint, _nodeData, _prefab, Spacing, SpawnDelay, RaycastHeightOffset, GroundMask);

    }

    public void Stop()
    {
        _hasFired = false;
    }
}
