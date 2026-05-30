using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class StatusEffectManager : MonoBehaviour
{
    [System.Serializable]
    private struct EffectEntry
    {
        public StatusEffectType type;
        public StatusEffectSO effectSO;
    }

    [SerializeField] private List<EffectEntry> effectsList;
    private Dictionary<StatusEffectType, StatusEffectSO> _effectsDict;

    private Dictionary<StatusEffectType, StatusEffectSO> _active = new();
    private Dictionary<StatusEffectType, StatusEffectSO> _cache = new();

    [SerializeField] private float interval = 0.1f;
    private float _timer;

    public UnityAction<StatusEffectSO, float> OnEffectActivated;
    public UnityAction<StatusEffectSO> OnEffectDeactivated;
    public UnityAction<StatusEffectSO, float, float> OnEffectUpdated;

    private void Awake()
    {
        _effectsDict = effectsList.ToDictionary(e => e.type, e => e.effectSO);
    }

    public void TriggerBuildup(StatusEffectType type, float amount)
    {
        if (!_effectsDict.ContainsKey(type)) return;

        if (!_active.ContainsKey(type))
        {
            var instance = GetOrCreateInstance(type);
            _active[type] = instance;
            OnEffectActivated?.Invoke(instance, instance.GetDurationNormalized());
        }

        if (!_active[type].isEffectActive)
        {
            _active[type].AddBuildup(amount, gameObject);
            OnEffectUpdated?.Invoke(_active[type],
                _active[type].GetThresholdNormalized(),
                _active[type].GetDurationNormalized());
        }
    }

    private StatusEffectSO GetOrCreateInstance(StatusEffectType type)
    {
        if (!_cache.ContainsKey(type))
            _cache[type] = Instantiate(_effectsDict[type]);
        return _cache[type];
    }

    private void UpdateEffects()
    {
        foreach (var kv in _active.ToList())
        {
            kv.Value.UpdateCall(gameObject, interval);
            OnEffectUpdated?.Invoke(kv.Value,
                kv.Value.GetThresholdNormalized(),
                kv.Value.GetDurationNormalized());

            if (kv.Value.CanRemoveVisual())
                RemoveEffect(kv.Key);
        }
    }

    public void RemoveEffect(StatusEffectType type)
    {
        if (!_active.ContainsKey(type)) return;
        _active[type].RemoveEffect(gameObject);
        OnEffectDeactivated?.Invoke(_active[type]);
        _active.Remove(type);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            UpdateEffects();
            _timer = 0f;
        }
    }
}