using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private Potion testPotion;

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            testPotion.AddToInventory();
    }
}
