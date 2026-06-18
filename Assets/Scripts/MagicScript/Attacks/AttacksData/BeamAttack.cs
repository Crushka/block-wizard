//using UnityEngine;

//public class BeamAttack : IAttack
//{
//    private NodeBase _nodeData;
//    private GameObject _prefab;

//    private const float DefaultRange = 20f;
//    private const float DamageInterval = 0.1f;

//    private static readonly LayerMask HitMask = LayerMask.GetMask("Enemy", "Default");

//    private GameObject _activeBeam;
//    private BeamProjectile _beamScript;
//    private float _damageTimer;

//    public void Init(NodeBase node, GameObject prefab)
//    {
//        _nodeData = node;
//        _prefab = prefab;
//    }

//    public void Cast(Transform spawnPoint)
//    {
//        if (_activeBeam == null)
//            CreateBeam(spawnPoint);

//        if (_beamScript == null) return;

//        float range = _nodeData?.Range ?? DefaultRange;

//        Vector3 endPoint;
//        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out RaycastHit hit, range, HitMask))
//        {
//            endPoint = hit.point;

//            _damageTimer -= Time.deltaTime;
//            if (_damageTimer <= 0f)
//            {
//                _damageTimer = DamageInterval;
//                float damage = _nodeData?.Damage ?? 10f;
//                Debug.Log($"[BeamAttack] Hit {hit.collider.name} for {damage} dmg");
//            }
//        }
//        else
//        {
//            endPoint = spawnPoint.position + spawnPoint.forward * range;
//            _damageTimer = 0f;
//        }

//        _beamScript.UpdateBeam(spawnPoint.position, endPoint);
//    }

//    public void Stop()
//    {
//        _damageTimer = 0f;

//        if (_activeBeam != null)
//        {
//            Object.Destroy(_activeBeam);
//            _activeBeam = null;
//            _beamScript = null;
//        }
//    }

//    private void CreateBeam(Transform spawnPoint)
//    {
//        _activeBeam = Object.Instantiate(_prefab, spawnPoint.position, Quaternion.identity);
//        _beamScript = _activeBeam.GetComponent<BeamProjectile>();

//        if (_beamScript == null)
//        {
//            Debug.LogError("[BeamAttack] на префабе не найден компонент BeamProjectile");
//            Object.Destroy(_activeBeam);
//            _activeBeam = null;
//        }

//        _beamScript.SetupVisual(_nodeData);
//    }
//}

using UnityEngine;

public class BeamAttack : IAttack
{
    private NodeBase _nodeData;
    private GameObject _prefab;

    private const float DefaultRange = 20f;
    private const float DefaultDamage = 10f;

    private GameObject _activeBeam;
    private ContinuousBeam _beamScript; // Ссылаемся на базовый класс лазера

    public void Init(NodeBase node, GameObject prefab)
    {
        _nodeData = node;
        _prefab = prefab;
    }

    public void Cast(Transform spawnPoint)
    {
        if (_activeBeam == null)
            CreateBeam(spawnPoint);

        if (_beamScript == null) return;

        // Синхронизируем позицию и направление объекта лазера с точкой спавна
        _activeBeam.transform.position = spawnPoint.position;
        _activeBeam.transform.rotation = spawnPoint.rotation;

        // Передаем управление логике лазера (она сама пустит SphereCast и нанесет урон)
        _beamScript.TickBeam();
    }

    public void Stop()
    {
        if (_activeBeam != null)
        {
            Object.Destroy(_activeBeam);
            _activeBeam = null;
            _beamScript = null;
        }
    }

    private void CreateBeam(Transform spawnPoint)
    {
        // Создаем луч в точке спавна
        _activeBeam = Object.Instantiate(_prefab, spawnPoint.position, spawnPoint.rotation);
        _beamScript = _activeBeam.GetComponent<ContinuousBeam>();

        if (_beamScript == null)
        {
            Debug.LogError("[BeamAttack] На префабе не найден компонент, наследуемый от ContinuousBeam!");
            Object.Destroy(_activeBeam);
            _activeBeam = null;
            return;
        }

        // Передаем параметры урона и дальности в логику луча
        float damage = _nodeData?.Damage ?? DefaultDamage;
        float range = _nodeData?.Range ?? DefaultRange;
        _beamScript.Setup(damage, range);

        // Настраиваем визуал луча
        _beamScript.SetupVisual(_nodeData);
    }
}
