//using UnityEngine;

//public class ThunderAttack : IAttack
//{
//    private NodeBase _nodeData;
//    private GameObject _prefab;

//    private const float CylinderRadius = 1.5f;
//    private const float DefaultRange = 15f;

//    private static readonly LayerMask EnemyMask = LayerMask.GetMask("Enemy");

//    private GameObject _activeBolt;
//    private ThunderProjectile _boltScript;
//    private bool _isCasting;

//    public void Init(NodeBase node, GameObject prefab)
//    {
//        _nodeData = node;
//        _prefab = prefab;
//    }

//    public void Cast(Transform spawnPoint)
//    {
//        if (_activeBolt == null)
//            CreateBolt(spawnPoint);

//        float range = _nodeData?.Range ?? DefaultRange;
//        Vector3 targetPos = ThunderTargetFinder.Find(
//            spawnPoint.position,
//            spawnPoint.forward,
//            range,
//            CylinderRadius,
//            EnemyMask
//            );

//        _boltScript?.UpdateBolt(spawnPoint.position, targetPos);

//    }


//    public void Stop()
//    {
//        _isCasting = false;

//        if (_activeBolt != null)
//        {
//            Object.Destroy(_activeBolt);
//            _activeBolt = null;
//            _boltScript = null;
//        }
//    }

//    private void CreateBolt(Transform spawnPoint)
//    {
//        _activeBolt = Object.Instantiate(_prefab, spawnPoint.position, Quaternion.identity);
//        _boltScript = _activeBolt.GetComponent<ThunderProjectile>();

//        if (_boltScript == null)
//        {
//            Debug.LogError("[ThunderAttack] префаб не найден ThunderProjectile");
//            Object.Destroy(_activeBolt);
//            _activeBolt = null;
//            return;
//        }

//        float damage = _nodeData?.Damage * 0.5f ?? 10f;
//        _boltScript.Setup(damage, 0, _nodeData?.Range ?? DefaultRange);
//        _boltScript.SetupVisual(_nodeData);

//        _isCasting = true;
//    }
//}

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

        // Если префаб должен следовать за стволом оружия/рукой:
        _activeBolt.transform.position = spawnPoint.position;
        _activeBolt.transform.rotation = spawnPoint.rotation;

        // Просто заставляем луч работать. Он сам разберется со своим transform
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
        // Спавним с позицией и ротацией точки каста
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