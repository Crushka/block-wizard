public class NodeItem : Item 
{
    public ElementType nodeType;

    public override void AddToInventory()
    {
        var nodeInventory = InventoryManager.Instance;
        if (nodeInventory != null)
        {
            for (int i = 0; i < amount; i++)
                nodeInventory.CreateNode(nodeType);
        }
    }
}