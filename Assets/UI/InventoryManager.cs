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
        
        CreateNode(ElementType.Fire); 
        CreateNode(ElementType.Earth);
        CreateNode(ElementType.Lightning); 
        CreateNode(ElementType.Air);
        CreateNode(ElementType.Lightning); 
        CreateNode(ElementType.Water);
        CreateNode(ElementType.Earth); 
        CreateNode(ElementType.Lightning);
        CreateNode(ElementType.Water); 
        CreateNode(ElementType.Cold);
        CreateNode(ElementType.Water);

        CreateNode(ElementType.Water);
        CreateNode(ElementType.Water);
        CreateNode(ElementType.Water);
        CreateNode(ElementType.Water);
        CreateNode(ElementType.Earth);
        CreateNode(ElementType.Earth);



    }

    public void CreateNode(ElementType type)
    {
        GameObject obj = Instantiate(nodePrefab, inventoryContainer);
        NodeView view = obj.GetComponent<NodeView>();
        view.Initialize(new NodeModel(type));
        view.SetVisualState(false); 
    }

    public void MoveToInventory(NodeView nv)
    {
        if (nv.Data.type == ElementType.None) return;
        NodeEditorManager.Instance.RemoveNodeFromGraph(nv);
        nv.transform.SetParent(inventoryContainer);
        nv.SetVisualState(false);
    }
}