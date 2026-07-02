
using UnityEngine;

public class ThunderAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;
    private const float DefaultRange = 15f;

    private GameObject _activeBolt;
    private ContinuousBeam _beamScript;

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_activeBolt == null)
            CreateBolt(spawnPoint);

        _activeBolt.transform.position = spawnPoint.position;
        _activeBolt.transform.rotation = spawnPoint.rotation;

        _beamScript?.TickBeam();
    }

    public void Stop()
    {
        if (_activeBolt != null)
        {
            Object.Destroy(_activeBolt);
        }

        _activeBolt = null;
        _beamScript = null;
    }

    private void CreateBolt(Transform spawnPoint)
    {
        _activeBolt = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        _beamScript = _activeBolt.GetComponent<ContinuousBeam>();

        if (_beamScript == null)
        {
            Debug.LogError("[ThunderAttack] На префабе не найден ContinuousBeam!");
            Object.Destroy(_activeBolt);
            return;
        }

        float damage = _nodeData?.Damage * 0.5f ?? 10f;
        float range = _nodeData?.Range ?? DefaultRange;

        _beamScript.Setup(damage, range);
        _beamScript.SetupVisual(_nodeData);
    }
}