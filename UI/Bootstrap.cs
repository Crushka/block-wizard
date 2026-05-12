using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public InventoryManager inventory;
    public NodeEditorManager editor;

    void Start()
    {
        if (inventory != null) inventory.InitInventory();
        if (editor != null) editor.InitEditor();
    }
}