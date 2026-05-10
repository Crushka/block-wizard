using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject connectionPrefab;
    public RectTransform inventoryPanel; 
    public RectTransform nodeEditorField; 

    void Start()
    {
        if (NodeEditorManager.Instance != null) {
            NodeEditorManager.Instance.nodeContainer = nodeEditorField;
            NodeEditorManager.Instance.connectionPrefab = connectionPrefab;
        }

        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.inventoryContainer = inventoryPanel;
            InventoryManager.Instance.nodePrefab = nodePrefab;
            InventoryManager.Instance.InitInventory();
        }

        if (NodeEditorManager.Instance != null) {
            NodeEditorManager.Instance.InitEditor(); 
        }
    }
}