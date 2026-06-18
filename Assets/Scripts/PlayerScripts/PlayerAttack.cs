using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class AttackTypeHelper
{
    private static readonly HashSet<AttackType> HoldAttacks = new()
    {
        AttackType.Spray,
        AttackType.Beam,
        AttackType.Stream,
        AttackType.Thunder
    };

    public static bool IsHoldAttack(AttackType type)
        => HoldAttacks.Contains(type);
}

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public CameraController cameraController;
    public PlayerController playerController;
    public BookInteraction bookInteraction;
    public Animator playerAnimator;
    public RawImage aimMarker;
    public SpellCaster spellCaster;

    [Header("Система зарядов")]
    [Tooltip("AttackChargeSystem должен быть на этом же объекте")]
    public AttackChargeSystem chargeSystem;

    [Header("Настройки прицеливания (Атаки)")]
    public float aimDistance = 1.5f;
    public Vector3 aimCameraOffset = new Vector3(0.5f, 0.5f, 0f);
    public float transitionSpeed = 9.5f;

    private InputAction _aimAction;
    private InputAction _fireAction;

    [HideInInspector] public bool isAiming;
    private bool _isFiring;
    private float _originalDistance;
    private bool _isDistanceCached;
    private float _lastCastTime = -Mathf.Infinity;

    void Awake()
    {
        _aimAction = new InputAction("Aim", InputActionType.Button);
        _aimAction.AddBinding("<Mouse>/rightButton");
        _aimAction.AddBinding("<Gamepad>/leftTrigger");

        _fireAction = new InputAction("Fire", InputActionType.Button);
        _fireAction.AddBinding("<Mouse>/leftButton");
        _fireAction.AddBinding("<Gamepad>/rightTrigger");

        if (chargeSystem == null)
            chargeSystem = GetComponent<AttackChargeSystem>();
    }

    void OnEnable()
    {
        _aimAction.Enable();
        _aimAction.started += OnAimStart;
        _aimAction.canceled += OnAimCancel;

        _fireAction.Enable();
        _fireAction.started += OnFireStart;
        _fireAction.canceled += OnFireCancel;
    }

    void OnDisable()
    {
        _aimAction.started -= OnAimStart;
        _aimAction.canceled -= OnAimCancel;
        _aimAction.Disable();

        _fireAction.started -= OnFireStart;
        _fireAction.canceled -= OnFireCancel;
        _fireAction.Disable();
    }

    void Start()
    {
        if (cameraController != null)
        {
            _originalDistance = cameraController.distance;
            _isDistanceCached = true;
        }

        if (aimMarker != null)
            aimMarker.enabled = false;

        if (chargeSystem == null)
            Debug.LogError($"[PlayerAttack] На объекте {gameObject.name} не найдена AttackChargeSystem! Стрельба работать не будет.");
    }

    void Update()
    {
        HandleFiring();
        HandleCameraAim();
    }

    private void HandleFiring()
    {
        if (!_isFiring || !isAiming) return;
        if (spellCaster == null || chargeSystem == null) return;

        AttackType currentType = GetCurrentAttackType();

        if (AttackTypeHelper.IsHoldAttack(currentType))
        {
            if (!chargeSystem.IsOverloaded)
            {
                spellCaster.Cast();
            }
            else
            {
                spellCaster.StopCast();
                _isFiring = false;
            }
        }
        else if (chargeSystem.TryFire())
        {
            spellCaster.Cast();
        }
    }

    private void HandleCameraAim()
    {
        if (cameraController == null) return;

        if (isAiming)
        {
            cameraController.distance = Mathf.Lerp(cameraController.distance, aimDistance, Time.deltaTime * transitionSpeed);
            cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, aimCameraOffset, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Возвращаем камеру назад только если у нас сохранена исходная дистанция
            if (_isDistanceCached)
            {
                if (Mathf.Abs(cameraController.distance - _originalDistance) > 0.01f)
                {
                    cameraController.distance = Mathf.Lerp(cameraController.distance, _originalDistance, Time.deltaTime * transitionSpeed);
                }
                else
                {
                    cameraController.distance = _originalDistance; // Присваиваем камере точное значение
                    _isDistanceCached = false; // Сбрасываем флаг, когда камера вернулась на место
                }
            }

            cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, Vector3.zero, Time.deltaTime * transitionSpeed);
        }
    }

    private void OnFireStart(InputAction.CallbackContext ctx)
    {
        if (!isAiming || chargeSystem == null) return;

        _isFiring = true;

        AttackType currentType = GetCurrentAttackType();
        if (AttackTypeHelper.IsHoldAttack(currentType))
        {
            if (!chargeSystem.TryStartHold())
            {
                _isFiring = false;
            }
        }
    }

    private void OnFireCancel(InputAction.CallbackContext ctx)
    {
        _isFiring = false;

        if (chargeSystem != null)
        {
            AttackType currentType = GetCurrentAttackType();
            if (AttackTypeHelper.IsHoldAttack(currentType))
                chargeSystem.StopHold();
        }

        spellCaster?.StopCast();
    }

    private void OnAimStart(InputAction.CallbackContext ctx)
    {
        if (bookInteraction != null && bookInteraction.IsReading) return;

        isAiming = true;
        if (aimMarker != null) aimMarker.enabled = true;

        if (bookInteraction != null) bookInteraction.canRead = false;
        if (playerController != null) playerController.isAiming = true;

        if (cameraController != null)
        {
            cameraController.isAiming = true;

            if (!_isDistanceCached)
            {
                _originalDistance = cameraController.distance;
                _isDistanceCached = true;
            }
        }

        if (playerAnimator != null)
            playerAnimator.SetBool("IsAiming", true);
    }

    private void OnAimCancel(InputAction.CallbackContext ctx)
    {
        if (!isAiming) return;

        isAiming = false;
        if (aimMarker != null) aimMarker.enabled = false;

        if (bookInteraction != null) bookInteraction.canRead = true;
        if (playerController != null) playerController.isAiming = false;
        if (cameraController != null) cameraController.isAiming = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsAiming", false);

        if (chargeSystem != null)
            chargeSystem.StopHold();

        spellCaster?.StopCast();
    }

    private AttackType GetCurrentAttackType()
    {
        return spellCaster?.CurrentSpellNode?.GetDominantAttack() ?? AttackType.Ball;
    }
}
