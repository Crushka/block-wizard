

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

        CreateNode(ElementType.Water);
        CreateNode(ElementType.Fire);
        CreateNode(ElementType.Air);
        CreateNode(ElementType.Lightning);
        CreateNode(ElementType.Ice);
        CreateNode(ElementType.Earth);
        CreateNode(ElementType.Cold);
    }

    public void CreateNode(ElementType type)
    {
        GameObject obj = Instantiate(nodePrefab, inventoryContainer);
        NodeView view = obj.GetComponent<NodeView>();
        view.Initialize(new NodeModel(type));
        view.SetVisualState(false);
    }

    public NodeView SpawnDragClone(NodeView sourceView, Canvas canvas)
    {
        GameObject clone = Instantiate(nodePrefab, canvas.transform);
        NodeView cloneView = clone.GetComponent<NodeView>();
        cloneView.Initialize(new NodeModel(sourceView.Data.type));
        cloneView.SetVisualState(true);

        RectTransform cloneRt = clone.GetComponent<RectTransform>();
        RectTransform sourceRt = sourceView.GetComponent<RectTransform>();
        cloneRt.position = sourceRt.position;

        return cloneView;
    }

    public void DestroyDragClone(NodeView cloneView)
    {
        if (cloneView != null)
            Destroy(cloneView.gameObject);
    }

    public void MoveToInventory(NodeView nv)
    {
        if (nv == null) return;
        if (nv.Data.type == ElementType.None) return;
        Destroy(nv.gameObject);
    }
}