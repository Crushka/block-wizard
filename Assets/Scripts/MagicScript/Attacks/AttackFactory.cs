
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AttackType
{
    Spray,
    Ball,
    Thunder,
    Stream,
    CircularWave,
    Spike,
    Beam
}

[System.Serializable]
public class AttackConfig
{
    public AttackType type;
    public GameObject prefab;
}

public class AttackFactory : MonoBehaviour
{
    [SerializeField] private List<AttackConfig> _attackConfigs = new();

    private Dictionary<AttackType, GameObject> _prefabMap;
    private Dictionary<AttackType, Func<IAttack>> _creators;
    private readonly Dictionary<NodeBase, IAttack> _cache = new();

    private void Awake()
    {
        _prefabMap = _attackConfigs.ToDictionary(c => c.type, c => c.prefab);

        _creators = new()
        {
            { AttackType.Spray,        () => new SprayAttack() },
            { AttackType.Ball,         () => new BallAttack() },
            { AttackType.Thunder,      () => new ThunderAttack() },
            { AttackType.Stream,       () => new StreamAttack() },
            { AttackType.CircularWave, () => new CircleWaveAttack() },
            { AttackType.Spike,        () => new SpikeAttack() },
            { AttackType.Beam,         () => new BeamAttack() }
        };
    }

    public IAttack GetAttack(NodeBase node)
    {
        if (_cache.TryGetValue(node, out IAttack cached))
            return cached;

        var type = node.GetDominantAttack();

        if (!_creators.TryGetValue(type, out Func<IAttack> creator))
        {
            Debug.LogError($"[AttackFactory] Нет создателя для {type}");
            creator = () => new BallAttack();
            type = AttackType.Ball;
        }

        if (!_prefabMap.TryGetValue(type, out GameObject prefab))
        {
            Debug.LogError($"[AttackFactory] Нет префаба для {type}");
            return null;
        }

        IAttack attack = creator();
        attack.Init(node, prefab);

        _cache[node] = attack;
        return attack;
    }
}