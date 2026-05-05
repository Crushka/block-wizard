using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("Связи")]
    public CameraController cameraController;
    public PlayerController playerController;
    public BookInteraction bookInteraction;
    public Animator playerAnimator;
    public RawImage aimMarker;

    [Header("Настройки прицеливания (Атаки)")]
    public float aimDistance = 1.5f;
    public Vector3 aimCameraOffset = new Vector3(0.5f, 0.5f, 0f);
    public float transitionSpeed = 9.5f;

    private InputAction aimAction;
    private bool isAiming = false;
    private float originalDistance;

    void Awake()
    {
        aimMarker.enabled = false;
        aimAction = new InputAction("Aim", InputActionType.Button);
        aimAction.AddBinding("<Mouse>/rightButton");
        aimAction.AddBinding("<Gamepad>/leftTrigger");
    }

    void OnEnable()
    {
        aimAction.Enable();
        aimAction.started += OnAimStart;
        aimAction.canceled += OnAimCancel;
    }

    void OnDisable()
    {
        aimAction.started -= OnAimStart;
        aimAction.canceled -= OnAimCancel;
        aimAction.Disable();
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
        aimMarker.enabled = true;

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
        aimMarker.enabled = false;

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