//using UnityEngine;

//public enum StatusEffectType { Bleed, Burn, Slow }

//public abstract class StatusEffectSO : ScriptableObject
//{
//    public StatusEffectType statusEffectType;

//    [Header("Buildup")]
//    public float activationThreshold;
//    public float thresholdReductionPerSecond = 1f;
//    public float thresholdReductionMultiplier = 1f;

//    [Header("Effect")]
//    public float activeDuration;
//    public float tickInterval = 0.5f;
//    public GameObject visualEffectPrefab;

//    [System.NonSerialized] public bool isBuildupVisible;
//    [System.NonSerialized] public bool isEffectActive;

//    private float _currentThreshold;
//    private float _remainingDuration;
//    private float _tickCooldown;
//    private GameObject _vfxInstance;

//    public virtual void AddBuildup(float amount, GameObject target)
//    {
//        isBuildupVisible = true;
//        _currentThreshold += amount;

//        if (_currentThreshold >= activationThreshold)
//            ApplyEffect(target);
//    }

//    public virtual void ApplyEffect(GameObject target)
//    {
//        isEffectActive = true;
//        _remainingDuration = activeDuration;

//        if (visualEffectPrefab != null && _vfxInstance == null)
//            _vfxInstance = Object.Instantiate(visualEffectPrefab, target.transform);
//    }

//    public virtual void UpdateCall(GameObject target, float delta)
//    {
//        if (isEffectActive)
//        {
//            isBuildupVisible = false;
//            _remainingDuration -= delta;

//            if (_remainingDuration <= 0f)
//                isEffectActive = false;
//        }
//        else
//        {
//            _currentThreshold -= delta * thresholdReductionPerSecond * thresholdReductionMultiplier;
//            if (_currentThreshold <= 0f)
//                isBuildupVisible = false;
//        }

//        _tickCooldown += delta;
//        if (_tickCooldown >= tickInterval)
//        {
//            if (isEffectActive) OnTick(target);
//            _tickCooldown = 0f;
//        }
//    }

//    protected virtual void OnTick(GameObject target) { }

//    public virtual void RemoveEffect(GameObject target)
//    {
//        isEffectActive = false;
//        _currentThreshold = 0f;
//        _remainingDuration = 0f;

//        if (_vfxInstance != null)
//        {
//            Object.Destroy(_vfxInstance);
//            _vfxInstance = null;
//        }
//    }

//    public bool CanRemoveVisual() => !(isEffectActive || isBuildupVisible);
//    public float GetThresholdNormalized() => _currentThreshold / activationThreshold;
//    public float GetDurationNormalized() => _remainingDuration / activeDuration;
//}

using UnityEngine;

public enum StatusEffectType { Bleed, Burn, Slow }

public abstract class StatusEffectSO : ScriptableObject
{
    public StatusEffectType statusEffectType;

    [Header("Buildup")]
    public float activationThreshold = 100f;
    public float thresholdReductionPerSecond = 1f;
    public float thresholdReductionMultiplier = 1f;

    [Header("Effect")]
    public float activeDuration = 3.7f;
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
        if (isEffectActive)
        {
            Debug.Log($"[StatusEffect] {statusEffectType} — buildup проигнорирован, эффект уже активен");
            return;
        }

        isBuildupVisible = true;
        _currentThreshold += amount;
        Debug.Log($"[StatusEffect] {statusEffectType} — buildup: {_currentThreshold:F1} / {activationThreshold} ({GetThresholdNormalized() * 100:F0}%)");

        if (_currentThreshold >= activationThreshold)
            ApplyEffect(target);
    }

    public virtual void ApplyEffect(GameObject target)
    {
        isEffectActive = true;
        isBuildupVisible = false;
        _currentThreshold = 0f;
        _remainingDuration = activeDuration;
        _tickCooldown = 0f;

        Debug.Log($"[StatusEffect] {statusEffectType} — ЭФФЕКТ АКТИВИРОВАН на {target.name}, длительность: {activeDuration}с");

        if (visualEffectPrefab != null && _vfxInstance == null)
            _vfxInstance = Object.Instantiate(visualEffectPrefab, target.transform);
    }

    public virtual void UpdateCall(GameObject target, float delta)
    {
        if (isEffectActive)
        {
            _remainingDuration -= delta;

            if (_remainingDuration <= 0f)
            {
                isEffectActive = false;
                DestroyVFX();
                Debug.Log($"[StatusEffect] {statusEffectType} — эффект закончился на {target.name}");
            }
        }
        else
        {
            if (_currentThreshold > 0f)
            {
                float before = _currentThreshold;
                _currentThreshold -= delta * thresholdReductionPerSecond * thresholdReductionMultiplier;

                if (_currentThreshold <= 0f)
                {
                    _currentThreshold = 0f;
                    isBuildupVisible = false;
                    Debug.Log($"[StatusEffect] {statusEffectType} — накопление полностью спало");
                }
                else
                {
                    Debug.Log($"[StatusEffect] {statusEffectType} — decay: {before:F1} → {_currentThreshold:F1}");
                }
            }
        }

        if (isEffectActive)
        {
            _tickCooldown += delta;
            if (_tickCooldown >= tickInterval)
            {
                Debug.Log($"[StatusEffect] {statusEffectType} — тик на {target.name}");
                OnTick(target);
                _tickCooldown = 0f;
            }
        }
    }

    public virtual void RemoveEffect(GameObject target)
    {
        Debug.Log($"[StatusEffect] {statusEffectType} — принудительно снят с {target.name}");
        isEffectActive = false;
        isBuildupVisible = false;
        _currentThreshold = 0f;
        _remainingDuration = 0f;
        _tickCooldown = 0f;
        DestroyVFX();
    }
    protected virtual void OnTick(GameObject target) { }

    private void DestroyVFX()
    {
        if (_vfxInstance != null)
        {
            Object.Destroy(_vfxInstance);
            _vfxInstance = null;
        }
    }

    public bool CanRemoveVisual() => !(isEffectActive || isBuildupVisible);
    public float GetThresholdNormalized() => Mathf.Clamp01(_currentThreshold / activationThreshold);
    public float GetDurationNormalized() => Mathf.Clamp01(_remainingDuration / activeDuration);
}