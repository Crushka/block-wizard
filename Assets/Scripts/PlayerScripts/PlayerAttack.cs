using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public CameraController cameraController;
    public PlayerController playerController;
    public BookInteraction bookInteraction;
    public Animator playerAnimator;
    public RawImage aimMarker;
    public SpellCaster spellCaster; [Header("Настройки прицеливания (Атаки)")]
    public float aimDistance = 1.5f;
    public Vector3 aimCameraOffset = new Vector3(0.5f, 0.5f, 0f);
    public float transitionSpeed = 9.5f;

    private InputAction _aimAction;
    private InputAction _fireAction;

    private bool _isAiming;
    private bool _isFiring;
    private float _originalDistance;

    void Awake()
    {
        _aimAction = new InputAction("Aim", InputActionType.Button);
        _aimAction.AddBinding("<Mouse>/rightButton");
        _aimAction.AddBinding("<Gamepad>/leftTrigger");

        _fireAction = new InputAction("Fire", InputActionType.Button);
        _fireAction.AddBinding("<Mouse>/leftButton");
        _fireAction.AddBinding("<Gamepad>/rightTrigger");
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
        }

        if (aimMarker != null)
            aimMarker.enabled = false;
    }

    void Update()
    {
        if (_isFiring && _isAiming)
            spellCaster?.Cast();

        if (cameraController == null) return;

        if (_isAiming)
        {
            cameraController.distance = Mathf.Lerp(cameraController.distance, aimDistance, Time.deltaTime * transitionSpeed);
            cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, aimCameraOffset, Time.deltaTime * transitionSpeed);
        }
        else
        {
            if (Mathf.Abs(cameraController.distance - _originalDistance) > 0.01f)
            {
                cameraController.distance = Mathf.Lerp(cameraController.distance, _originalDistance, Time.deltaTime * transitionSpeed);
            }
            else
            {
                _originalDistance = cameraController.distance;
            }

            cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, Vector3.zero, Time.deltaTime * transitionSpeed);
        }

    }

    private void OnFireStart(InputAction.CallbackContext ctx)
    {
        _isFiring = true;
    }

    private void OnFireCancel(InputAction.CallbackContext ctx)
    {
        _isFiring = false;
        spellCaster?.StopCast();
    }

    private void OnAimStart(InputAction.CallbackContext ctx)
    {
        if (bookInteraction != null && bookInteraction.IsReading) return;

        _isAiming = true;
        if (aimMarker != null) aimMarker.enabled = true;

        if (bookInteraction != null) bookInteraction.canRead = false;
        if (playerController != null) playerController.isAiming = true;

        if (cameraController != null)
        {
            cameraController.isAiming = true;
            _originalDistance = cameraController.distance;
        }

        if (playerAnimator != null)
            playerAnimator.SetBool("IsAiming", true);
    }

    private void OnAimCancel(InputAction.CallbackContext ctx)
    {
        if (!_isAiming) return;

        _isAiming = false;
        if (aimMarker != null) aimMarker.enabled = false;

        if (bookInteraction != null) bookInteraction.canRead = true;
        if (playerController != null) playerController.isAiming = false;
        if (cameraController != null) cameraController.isAiming = false;

        if (playerAnimator != null)
            playerAnimator.SetBool("IsAiming", false);
    }
}