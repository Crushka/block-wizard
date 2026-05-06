using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Связи")]
    public CameraController cameraController;
    public PlayerController playerController;
    public BookInteraction bookInteraction;
    public Animator playerAnimator;
    public SpellCaster spellCaster;
    [Header("Настройки прицеливания (Атаки)")]
    public float aimDistance = 2.0f; 
    public Vector3 aimCameraOffset = new Vector3(0.5f, 1.5f, 0f);
    public float transitionSpeed = 8.0f;

    private InputAction aimAction;
    private InputAction fireAction;
    private bool isAiming = false;
    private float originalDistance;

    void Awake()
    {
        aimAction = new InputAction("Aim", InputActionType.Button);
        aimAction.AddBinding("<Mouse>/rightButton");
        aimAction.AddBinding("<Gamepad>/leftTrigger");

        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");
    }

    void OnEnable()
    {
        aimAction.Enable();
        aimAction.started += OnAimStart;
        aimAction.canceled += OnAimCancel;

        fireAction.Enable();
        fireAction.started += OnFire;
        fireAction.canceled += OnFireCancel;
    }

    void OnDisable()
    {
        aimAction.started -= OnAimStart;
        aimAction.canceled -= OnAimCancel;
        aimAction.Disable();

        fireAction.started -= OnFire;
        fireAction.Disable();
    }
    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (!isAiming) return;
        spellCaster?.Cast();
    }
    private void OnFireCancel(InputAction.CallbackContext ctx)
    {
        spellCaster?.StopCast();
    }

    void Start()
    {
        if (cameraController != null)
        {
            originalDistance = cameraController.distance;
        }
    }

    private void OnAimStart(InputAction.CallbackContext ctx)
    {
        if (bookInteraction != null && bookInteraction.IsReading) return;

        isAiming = true;

        if (bookInteraction != null) bookInteraction.canRead = false;

        if (playerController != null) playerController.isAiming = true;

        if (cameraController != null)
        {
            cameraController.isAiming = true;
            originalDistance = cameraController.distance;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAiming", true);
        }
    }

    private void OnAimCancel(InputAction.CallbackContext ctx)
    {
        if (!isAiming) return;
        isAiming = false;

        if (bookInteraction != null) bookInteraction.canRead = true;

        if (playerController != null) playerController.isAiming = false;
        if (cameraController != null) cameraController.isAiming = false;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAiming", false);
        }
    }

    void Update()
    {
        if (cameraController == null) return;

        float targetDistance = isAiming ? aimDistance : originalDistance;
        Vector3 targetOffset = isAiming ? aimCameraOffset : Vector3.zero;

        cameraController.distance = Mathf.Lerp(cameraController.distance, targetDistance, Time.deltaTime * transitionSpeed);
        cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, targetOffset, Time.deltaTime * transitionSpeed);
    }
}