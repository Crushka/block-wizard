using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Цель орбиты")]
    public Transform target;

    [Header("Настройки дистанции (Зум)")]
    public float distance = 5.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;
    public float zoomSpeed = 0.01f;

    [Header("Настройки вращения")]
    public float sensitivity = 0.15f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 180f;

    [HideInInspector]
    public bool isControlEnabled = true;

    [HideInInspector] public bool isAiming = false;
    [HideInInspector] public Vector3 targetOffset = Vector3.zero;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private InputAction lookAction;

    void Awake()
    {
        lookAction = new InputAction("Look", InputActionType.Value);
        lookAction.AddBinding("<Mouse>/delta");
        lookAction.AddBinding("<Gamepad>/rightStick");
    }

    void OnEnable()
    {
        lookAction.Enable();
    }

    void OnDisable()
    {
        lookAction.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }

    void LateUpdate()
    {
        if (!isControlEnabled || target == null) return;

        Vector2 lookDelta = lookAction.ReadValue<Vector2>();
        currentX += lookDelta.x * sensitivity;
        currentY -= lookDelta.y * sensitivity;
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

        transform.rotation = GetOrbitRotation();
        transform.position = GetOrbitPosition();
    }

    public Quaternion GetOrbitRotation()
    {
        return Quaternion.Euler(currentY, currentX, 0);
    }

    public Vector3 GetOrbitPosition()
    {
        Vector3 worldOffset = target.TransformDirection(targetOffset);
        return target.position + worldOffset + GetOrbitRotation() * new Vector3(0.0f, 0.0f, -distance);
    }
}