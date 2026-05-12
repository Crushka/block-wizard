using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 6.0f;
    public float jumpForce = 8.0f;
    public float gravity = 20.0f;

    [Header("Настройки поворота")]
    public float rotationSpeed = 10.0f;

    [Header("Компоненты")]
    public Camera playerCamera;
    public Animator playerAnimator;

    [Header("Состояние")]
    public bool isMovementEnabled = true;

    [HideInInspector] public bool isAiming = false;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    private InputAction moveAction;
    private InputAction jumpAction;

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
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        bool isMoving = false;


        if (controller.isGrounded)
        {
            if (isMovementEnabled)
            {
                Vector2 input = moveAction.ReadValue<Vector2>();
                float h = input.x;
                float v = input.y;

                if (playerCamera != null)
                {
                    Vector3 camForward = playerCamera.transform.forward;
                    Vector3 camRight = playerCamera.transform.right;
                    camForward.y = 0;
                    camRight.y = 0;
                    camForward.Normalize();
                    camRight.Normalize();

                    moveDirection = (camForward * v + camRight * h).normalized;
                    moveDirection *= speed;
                }
                else
                {
                    moveDirection = new Vector3(h, 0, v).normalized * speed;
                }

                isMoving = moveDirection.magnitude > 0.1f;

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
                else if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                moveDirection.x = 0;
                moveDirection.z = 0;
            }

            if (jumpAction.WasPressedThisFrame() && isMovementEnabled)
            {
                moveDirection.y = jumpForce;
            }
        }
        else
        {
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z);
            isMoving = horizontalMove.magnitude > 0.1f && isMovementEnabled;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", isMoving);
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}