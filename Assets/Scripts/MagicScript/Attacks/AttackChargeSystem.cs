using UnityEngine;


public class AttackChargeSystem : MonoBehaviour
{
    [Header("Заряды")]
    [Tooltip("Максимальное количество зарядов")]
    public int maxCharges = 3;

    [Tooltip("Минимальный интервал между выстрелами (сек)")]
    public float fireInterval = 0.5f;

    [Header("Восстановление и перегрузка")]
    [Tooltip("Время перегрузки (сек) — нельзя атаковать совсем")]
    public float overloadCooldown = 3f;

    [Tooltip("Время восстановления 1 заряда после выстрела (сек)")]
    public float chargeRegenTime = 0.3f;

    [Header("Зажимные атаки (Spray, Beam)")]
    [Tooltip("Максимальное время непрерывного удержания до перегрузки (сек)")]
    public float maxHoldTime = 2f;

    public int CurrentCharges { get; private set; }
    public bool IsOverloaded { get; private set; }


    // тут кароч прогресс с нормализацией 
    public float OverloadProgress => IsOverloaded
        ? Mathf.Clamp01((_overloadEndTime - Time.time) / overloadCooldown)
        : 0f;

    public float NextChargeRegenProgress
    {
        get
        {
            if (CurrentCharges >= maxCharges || _nextRegenTime <= 0f) return 1f;
            float elapsed = Time.time - (_nextRegenTime - chargeRegenTime);
            return Mathf.Clamp01(elapsed / chargeRegenTime);
        }
    }

    private float _lastFireTime = -Mathf.Infinity;
    private float _nextRegenTime = -1f;
    private float _overloadEndTime;
    private float _holdStartTime = -1f;
    private bool _isHolding;

    void Awake()
    {
        CurrentCharges = maxCharges;
    }

    void Update()
    {
        TickOverload();
        TickRegeneration();
        TickHoldOverload();
    }

    public bool TryFire()
    {
        if (IsOverloaded) return false;

        if (Time.time - _lastFireTime < fireInterval) return false;

        if (CurrentCharges <= 0)
        {
            TriggerOverload();
            return false;
        }

        ConsumeCharge();
        return true;
    }

    public bool TryStartHold()
    {
        if (IsOverloaded) return false;
        if (CurrentCharges <= 0) { TriggerOverload(); return false; }

        _holdStartTime = Time.time;
        _isHolding = true;
        return true;
    }

    public void StopHold()
    {
        if (!_isHolding) return;
        _isHolding = false;

        ConsumeCharge();
        _holdStartTime = -1f;
    }

    private void ConsumeCharge()
    {
        CurrentCharges = Mathf.Max(0, CurrentCharges - 1);
        _lastFireTime = Time.time;

        if (CurrentCharges <= 0)
        {
            TriggerOverload();
        }
        else
        {
            ScheduleRegen();
        }
    }

    private void ScheduleRegen()
    {
        if (CurrentCharges < maxCharges && _nextRegenTime < 0f)
            _nextRegenTime = Time.time + chargeRegenTime;
    }

    private void TriggerOverload()
    {
        IsOverloaded = true;
        _overloadEndTime = Time.time + overloadCooldown;
        CurrentCharges = 0;
        _nextRegenTime = -1f;
        _isHolding = false;
    }

    private void TickOverload()
    {
        if (!IsOverloaded) return;
        if (Time.time >= _overloadEndTime)
        {
            IsOverloaded = false;
            CurrentCharges = maxCharges;
            _nextRegenTime = -1f;
        }
    }

    private void TickRegeneration()
    {
        if (IsOverloaded) return;
        if (CurrentCharges >= maxCharges) return;
        if (_nextRegenTime < 0f) return;

        if (Time.time >= _nextRegenTime)
        {
            CurrentCharges++;
            if (CurrentCharges < maxCharges)
                _nextRegenTime = Time.time + chargeRegenTime;
            else
                _nextRegenTime = -1f;
        }
    }

    private void TickHoldOverload()
    {
        if (!_isHolding) return;
        if (IsOverloaded) { _isHolding = false; return; }

        if (Time.time - _holdStartTime >= maxHoldTime)
        {
            _isHolding = false;
            TriggerOverload();
        }
    }
}