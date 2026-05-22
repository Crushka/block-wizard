using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 6.0f;
    public float aimSpeed = 3.0f;
    public float jumpForce = 8.0f;
    public float gravity = 20.0f;

    [Header("Настройки поворота")]
    public float rotationSpeed = 10.0f;

    [Header("Настройки рывка (Dash)")]
    public float dashSpeed = 10.0f;
    public float dashDuration = 1.0f;
    public float dashCooldown = 1.0f;
    public bool allowAirDash = false;

    [Header("Компоненты")]
    public Camera playerCamera;
    public Animator playerAnimator;

    [Header("Состояние")]
    public bool isMovementEnabled = true;

    [HideInInspector] public bool isAiming = false;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    private float animInputX = 0f;
    private float animInputY = 0f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection = Vector3.zero;

    void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value, binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/shift");
        dashAction.AddBinding("<Gamepad>/buttonEast");
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }

        bool isMoving = false;
        float h = 0f;
        float v = 0f;

        if (isMovementEnabled)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            h = input.x;
            v = input.y;

            bool canDash = allowAirDash || controller.isGrounded;

            if (dashAction.WasPressedThisFrame() && !isAiming && !isDashing && dashCooldownTimer <= 0 && canDash)
            {
                isDashing = true;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;

                if (playerCamera != null)
                {
                    Vector3 camForward = playerCamera.transform.forward;
                    Vector3 camRight = playerCamera.transform.right;
                    camForward.y = 0; camRight.y = 0;
                    camForward.Normalize(); camRight.Normalize();

                    if (input.magnitude > 0.1f)
                        dashDirection = (camForward * v + camRight * h).normalized;
                    else
                        dashDirection = transform.forward;
                }
                else
                {
                    if (input.magnitude > 0.1f)
                        dashDirection = new Vector3(h, 0, v).normalized;
                    else
                        dashDirection = transform.forward;
                }

                if (dashDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(dashDirection);
                }

                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("Dash");
                }
            }

            Vector3 horizontalMove = Vector3.zero;

            if (isDashing)
            {
                horizontalMove = dashDirection * dashSpeed;
                isMoving = false;
            }
            else
            {
                float currentSpeed = isAiming ? aimSpeed : speed;

                if (playerCamera != null)
                {
                    Vector3 camForward = playerCamera.transform.forward;
                    Vector3 camRight = playerCamera.transform.right;
                    camForward.y = 0; camRight.y = 0;
                    camForward.Normalize(); camRight.Normalize();

                    horizontalMove = (camForward * v + camRight * h).normalized * currentSpeed;
                }
                else
                {
                    horizontalMove = new Vector3(h, 0, v).normalized * currentSpeed;
                }

                isMoving = horizontalMove.magnitude > 0.1f;
            }

            moveDirection.x = horizontalMove.x;
            moveDirection.z = horizontalMove.z;

            if (!isDashing)
            {
                if (isAiming)
                {
                    if (playerCamera != null)
                    {
                        Vector3 camForward = playerCamera.transform.forward;
                        camForward.y = 0;
                        if (camForward.sqrMagnitude > 0.001f)
                        {
                            transform.rotation = Quaternion.LookRotation(camForward.normalized);
                        }
                    }
                }
                else if (horizontalMove.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(horizontalMove);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            moveDirection.x = 0;
            moveDirection.z = 0;
            isDashing = false;
        }

        if (controller.isGrounded)
        {
            if (moveDirection.y < 0.0f)
            {
                moveDirection.y = -2f;
            }

            if (jumpAction.WasPressedThisFrame() && isMovementEnabled && !isAiming && !isDashing)
            {
                moveDirection.y = jumpForce;

                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("Jump");
                }
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", isMoving);
            playerAnimator.SetBool("IsGrounded", controller.isGrounded);

            playerAnimator.SetBool("IsDashing", isDashing);

            animInputX = Mathf.Lerp(animInputX, h, Time.deltaTime * 10f);
            animInputY = Mathf.Lerp(animInputY, v, Time.deltaTime * 10f);

            playerAnimator.SetFloat("InputX", animInputX);
            playerAnimator.SetFloat("InputY", animInputY);
        }
    }

    
}