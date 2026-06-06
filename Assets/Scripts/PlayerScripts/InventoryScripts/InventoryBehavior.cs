using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryBehavior : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject hpUI;
    [SerializeField] private BookInteraction bookInteraction;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerComboAttack playerComboAttack;
    [SerializeField] private PlayerAttack playerAttack;

    [HideInInspector] public bool isInventoryOpen = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleInventory();
            ToggleHP();
            ToggleCanRead();
            ToggleCursorLock();
            ToggleCameraLock();
            TogglePlayerMovement();
            TogglePlayerAttack();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            if (bookInteraction.isReading)
            {
                Debug.Log("[InventoryBehavior] СТОП: книга открыта");
                return;
            }
            if (playerAttack.isAiming)
            {
                Debug.Log("[InventoryBehavior] СТОП: прицеливание активно");
                return;
            }
            bool isActive = inventoryUI.activeSelf;
            isInventoryOpen = !isActive;
            inventoryUI.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Пожалуйста, назначьте Inventory Object в инспекторе скрипта.", this);
        }
    }

    private void ToggleHP()
    {
        if (hpUI != null)
        {
            bool isActive = inventoryUI.activeSelf;
            hpUI.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Пожалуйста, назначьте HP Object в инспекторе скрипта.", this);
        }
    }

    private void ToggleCanRead()
    {
        if (inventoryUI != null && bookInteraction != null)
        {
            bool isActive = inventoryUI.activeSelf;
            bookInteraction.canRead = !isActive;
        }
        else
        {
            Debug.LogWarning("Пожалуйста, назначьте Inventory Object и Book Interaction в инспекторе скрипта.", this);
        }
    }

    private void ToggleCursorLock()
    {
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ToggleCameraLock()
    {
        if (isInventoryOpen)
        {
            cameraController.isControlEnabled = false;

        }
        else
        {
            cameraController.isControlEnabled = true;
        }
    }

    private void TogglePlayerMovement()
    {
        if (isInventoryOpen)
        {
            playerController.isMovementEnabled = false;
        }
        else
        {
            playerController.isMovementEnabled = true;
        }
    }

    private void TogglePlayerAttack()
    {
        if (isInventoryOpen)
        {
            playerComboAttack.canAttack = false;
        }
        else
        {
            playerComboAttack.canAttack = true;
        }
    }
}