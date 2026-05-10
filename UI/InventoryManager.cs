using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public RectTransform inventoryContainer;
    public GameObject nodePrefab;

    void Awake() => Instance = this;

    public void InitInventory()
    {
        if (inventoryContainer == null) return;
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);
        CreateNode(MagicType.Fire); CreateNode(MagicType.Water);
        CreateNode(MagicType.Earth); CreateNode(MagicType.Air);
        CreateNode(MagicType.Fire); CreateNode(MagicType.Water);
        CreateNode(MagicType.Earth); CreateNode(MagicType.Air);
    }

    public void CreateNode(MagicType type)
    {
        GameObject obj = Instantiate(nodePrefab, inventoryContainer);
        NodeView view = obj.GetComponent<NodeView>();

        view.Initialize(new NodeModel(type));

        view.SetVisualState(false); 
    }

    public void MoveToInventory(NodeView nv)
    {
        if (nv.Data.type == MagicType.Magic) return;
        
        string tId = nv.Data.id;
        if (NodeEditorManager.Instance != null) {
            foreach (var n in NodeEditorManager.Instance.Graph.Nodes)
                if (n.connectedIds.Contains(tId)) n.connectedIds.Remove(tId);
        }
        nv.Data.connectedIds.Clear();
        nv.Data.weight = int.MaxValue;
        
        nv.transform.SetParent(inventoryContainer);

        nv.SetVisualState(false); 
        
        if (NodeEditorManager.Instance != null) NodeEditorManager.Instance.RemoveNodeFromGraph(nv);
        nv.UpdateVisuals();
    }
}