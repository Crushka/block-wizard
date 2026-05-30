using UnityEngine;

public enum StatusEffectType { Burn, Ice, Bleed, Regeneration, Slow }

public abstract class StatusEffectSO : ScriptableObject
{
    public StatusEffectType statusEffectType;

    [Header("Buildup")]
    public float activationThreshold;
    public float thresholdReductionPerSecond = 1f;
    public float thresholdReductionMultiplier = 1f;

    [Header("Effect")]
    public float activeDuration;
    public float tickInterval = 0.5f;
    public GameObject visualEffectPrefab;

    [System.NonSerialized] public bool isBuildupVisible;
    [System.NonSerialized] public bool isEffectActive;

    private float _currentThreshold;
    private float _remainingDuration;
    private float _tickCooldown;
    private GameObject _vfxInstance;

    public virtual void AddBuildup(float amount, GameObject target)
    {
        isBuildupVisible = true;
        _currentThreshold += amount;

        if (_currentThreshold >= activationThreshold)
            ApplyEffect(target);
    }

    public virtual void ApplyEffect(GameObject target)
    {
        isEffectActive = true;
        _remainingDuration = activeDuration;

        if (visualEffectPrefab != null && _vfxInstance == null)
            _vfxInstance = Object.Instantiate(visualEffectPrefab, target.transform);
    }

    public virtual void UpdateCall(GameObject target, float delta)
    {
        if (isEffectActive)
        {
            isBuildupVisible = false;
            _remainingDuration -= delta;

            if (_remainingDuration <= 0f)
                isEffectActive = false;
        }
        else
        {
            _currentThreshold -= delta * thresholdReductionPerSecond * thresholdReductionMultiplier;
            if (_currentThreshold <= 0f)
                isBuildupVisible = false;
        }

        _tickCooldown += delta;
        if (_tickCooldown >= tickInterval)
        {
            if (isEffectActive) OnTick(target);
            _tickCooldown = 0f;
        }
    }

    protected virtual void OnTick(GameObject target) { }

    public virtual void RemoveEffect(GameObject target)
    {
        isEffectActive = false;
        _currentThreshold = 0f;
        _remainingDuration = 0f;

        if (_vfxInstance != null)
        {
            Object.Destroy(_vfxInstance);
            _vfxInstance = null;
        }
    }

    public bool CanRemoveVisual() => !(isEffectActive || isBuildupVisible);
    public float GetThresholdNormalized() => _currentThreshold / activationThreshold;
    public float GetDurationNormalized() => _remainingDuration / activeDuration;
}