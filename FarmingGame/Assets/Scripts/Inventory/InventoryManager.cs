using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject m_slotPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InventorySystem.current.onInventoryChangedEvent += OnUpdateInventory;
        DrawInventory();
    }

    private void OnUpdateInventory()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        DrawInventory();
    }

    private void DrawInventory()
    {
        foreach (InventoryItem item in InventorySystem.current.inventory)
        {
            if (item != null && item.data != null)
                AddInventorySlot(item);
        }
    }

    public void AddInventorySlot(InventoryItem item)
    {
        if (item == null || item.data == null || item.data.prefab == null)
        {
            Debug.LogWarning("InventoryItem or prefab is null, skipping slot.");
            return;
        }

        GameObject obj = Instantiate(m_slotPrefab);
        obj.transform.SetParent(transform, false);

        InventorySlot slot = obj.GetComponent<InventorySlot>();
        slot.Set(item);
    }
}
