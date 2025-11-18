using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // TEMPORARY FOR TESTING
    [Header("DEBUG GIVE ITEMS")]
    public Sprite beehiveIcon;
    public GameObject beehivePrefab;
    public Sprite flowerIcon;
    public GameObject flowerPrefab;

    public static Inventory Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public Sprite icon;
        public GameObject prefab;
        public int amount;

        public bool IsEmpty => prefab == null || amount <= 0;
    }

    [Header("Inventory Setup")]
    public int defaultSlotCount = 30;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxStackSize = 99;

    private void Awake()
    {
        Instance = this;

        // Ensure size
        while (slots.Count < defaultSlotCount)
            slots.Add(new InventorySlot());
    }

    // TESTING PURPOSES
    private void Start()
    {
        // TEMP: Give player 1 beehive and 1 flower, assign to hotbar
        if (beehivePrefab != null)
        {
            AddItem(beehiveIcon, beehivePrefab, 1);
            HotbarUI.Instance?.SetQuickSlot(0, beehiveIcon, beehivePrefab);
        }

        if (flowerPrefab != null)
        {
            AddItem(flowerIcon, flowerPrefab, 1);
            HotbarUI.Instance?.SetQuickSlot(1, flowerIcon, flowerPrefab);
        }

        InventoryUI.Instance?.Refresh();
    }



    // Add item  
    public bool AddItem(Sprite icon, GameObject prefab, int amount = 1, bool addToHotbar = false)
    {
        // 1. Try stacking based on prefab identity
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty && slots[i].prefab == prefab)
            {
                int space = maxStackSize - slots[i].amount;
                if (space > 0)
                {
                    int added = Mathf.Min(space, amount);
                    slots[i].amount += added;
                    amount -= added;

                    if (amount <= 0)
                    {
                        InventoryUI.Instance?.Refresh();

                        // Assign to hotbar if requested
                        if (addToHotbar) HotbarUI.Instance?.SetQuickSlot(HotbarUI.Instance.selectedHotbarIndex, icon, prefab);

                        return true;
                    }
                }
            }
        }

        // 2. Place into an empty slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].icon = icon;
                slots[i].prefab = prefab;
                slots[i].amount = Mathf.Min(amount, maxStackSize);
                amount -= maxStackSize;

                InventoryUI.Instance?.Refresh();

                // Assign to hotbar if requested
                if (addToHotbar) HotbarUI.Instance?.SetQuickSlot(HotbarUI.Instance.selectedHotbarIndex, icon, prefab);

                return true;
            }
        }

        return false; // inventory full
    }


    // Directly set item slot
    public void SetItem(int slotIndex, Sprite icon, GameObject prefab, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        slots[slotIndex].icon = icon;
        slots[slotIndex].prefab = prefab;
        slots[slotIndex].amount = Mathf.Min(amount, maxStackSize);

        InventoryUI.Instance?.Refresh();
    }

    // Consume from slot
    public bool ConsumeItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        if (slots[slotIndex].IsEmpty) return false;

        slots[slotIndex].amount -= amount;

        if (slots[slotIndex].amount <= 0)
            ClearSlot(slotIndex);

        InventoryUI.Instance?.Refresh();
        return true;
    }

    public void ClearSlot(int slotIndex)
    {
        slots[slotIndex].icon = null;
        slots[slotIndex].prefab = null;
        slots[slotIndex].amount = 0;

        InventoryUI.Instance?.Refresh();
    }
}
