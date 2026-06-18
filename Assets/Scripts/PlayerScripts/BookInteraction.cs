using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookInteraction : MonoBehaviour
{
    [Header("Связи")]
    public CameraController cameraController;
    public Transform bookViewPoint;
    public PlayerController playerController;
    public Animator playerAnimator;
    public SpellEditorUI spellEditorUI;
    public InventoryBehavior inventoryBehavior;

    [HideInInspector] public InfoManager infoManager;

    [Header("UI объекты")]
    [SerializeField] private GameObject playerUI;

    [Header("Настройки перехода")]
    public float transitionDuration = 1.0f;

    [Header("Настройки анимации (для синхронизации движения)")]
    public float grabDuration = 0.6f;
    public float releaseDuration = 0.6f;

    [HideInInspector] public bool isReading = false;
    private bool isTransitioning = false;

    private InputAction toggleReadAction;
    [HideInInspector] public bool canRead = true;
    public bool IsReading => isReading;

    void Awake()
    {
        infoManager = FindAnyObjectByType<InfoManager>();
        toggleReadAction = new InputAction("ToggleRead", InputActionType.Button);
        toggleReadAction.AddBinding("<Keyboard>/tab");
    }

    void OnEnable()
    {
        toggleReadAction.Enable();
        toggleReadAction.performed += OnToggleRead;
    }

    void OnDisable()
    {
        toggleReadAction.performed -= OnToggleRead;
        toggleReadAction.Disable();
    }

    private void OnToggleRead(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[BookInteraction] Tab нажат. canRead={canRead} isTransitioning={isTransitioning}");

        if (infoManager.isActive)
        {
            Debug.Log("[BookInteraction] СТОП: InfoManager открыт");
            return;
        }
        if (!canRead)
        {
            Debug.Log("[BookInteraction] СТОП: canRead=false");
            return;
        }
        if (inventoryBehavior != null && inventoryBehavior.isInventoryOpen)
        {
            Debug.Log("[BookInteraction] СТОП: инвентарь открыт");
            return;
        }
        if (bookViewPoint == null || cameraController == null || playerController == null)
        {
            Debug.Log($"[BookInteraction] СТОП: bookViewPoint={bookViewPoint} cam={cameraController} player={playerController}");
            return;
        }
        if (isTransitioning)
        {
            Debug.Log("[BookInteraction] СТОП: isTransitioning=true");
            return;
        }

        isReading = !isReading;
        playerUI.SetActive(!isReading);
        if (isReading && SpellSlotManager.Instance != null) SpellSlotManager.Instance.OnBookOpened();
        Debug.Log($"[BookInteraction] isReading={isReading}, spellEditorUI={spellEditorUI}");

        if (spellEditorUI != null)
            spellEditorUI.SetEditorOpen(isReading);
        else
            Debug.LogError("[BookInteraction] spellEditorUI == null назначь GameManager в инспекторе.");

        StartCoroutine(SyncAnimationAndMovement(isReading));
        StartCoroutine(TransitionCamera());
    }

    private IEnumerator SyncAnimationAndMovement(bool reading)
    {
        if (playerAnimator != null)
            playerAnimator.SetBool("IsReading", reading);

        if (reading)
        {
            playerController.isMovementEnabled = false;
            yield return new WaitForSeconds(grabDuration);
        }
        else
        {
            yield return new WaitForSeconds(releaseDuration);
            playerController.isMovementEnabled = true;
        }
    }

    private IEnumerator TransitionCamera()
    {
        isTransitioning = true;
        cameraController.isControlEnabled = false;

        float elapsed = 0f;
        Vector3 startPosition = cameraController.transform.position;
        Quaternion startRotation = cameraController.transform.rotation;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 targetPosition = isReading ? bookViewPoint.position : cameraController.GetOrbitPosition();
            Quaternion targetRotation = isReading ? bookViewPoint.rotation : cameraController.GetOrbitRotation();

            cameraController.transform.position = Vector3.Lerp(startPosition, targetPosition, easeT);
            cameraController.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, easeT);
            yield return null;
        }

        cameraController.transform.position = isReading ? bookViewPoint.position : cameraController.GetOrbitPosition();
        cameraController.transform.rotation = isReading ? bookViewPoint.rotation : cameraController.GetOrbitRotation();

        if (!isReading)
            cameraController.isControlEnabled = true;

        isTransitioning = false;
    }
}