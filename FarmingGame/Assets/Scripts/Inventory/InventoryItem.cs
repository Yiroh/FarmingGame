using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public InventoryItemData data { get; private set; }
    public int stackSize { get; private set; }

    public InventoryItem(InventoryItemData source)
    {
        data = source;
        stackSize = 1;
    }

    public void AddToStack() => stackSize++;
    public void RemoveFromStack() => stackSize--;
}
