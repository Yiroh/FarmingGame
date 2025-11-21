using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public Dictionary<InventoryItemData, InventoryItem> m_itemDictionary;
    public List<InventoryItem> inventory { get; private set; }

    public static InventorySystem current;

    public event Action onInventoryChangedEvent;

    [Header("Starting Inventory Items")]
    public StartingInventoryItem[] startingItems;

    private void Awake()
    {
        current = this;

        m_itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();
        inventory = new List<InventoryItem>();

        if (startingItems != null)
        {
            foreach (var entry in startingItems)
            {
                if (entry.itemData != null && entry.amount > 0)
                {
                    InventoryItem newItem = new InventoryItem(entry.itemData);
                    
                    // Add the extra amount
                    for (int i = 1; i < entry.amount; i++)
                    {
                        newItem.AddToStack();
                    }

                    m_itemDictionary.Add(entry.itemData, newItem);
                }
            }
        }
    }

    private void Start()
    {
        if (InventorySystem.current == null) return;

        // Populate inventory from the dictionary
        inventory.Clear();
        foreach (var kvp in m_itemDictionary)
        {
            inventory.Add(kvp.Value);
        }

        // Notify UI to draw
        onInventoryChangedEvent?.Invoke();
    }

    public InventoryItem Get(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            return value;
        }
        return null;
    }

    public void Add(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.AddToStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventory.Add(newItem);
            m_itemDictionary.Add(referenceData, newItem);
        }

        onInventoryChangedEvent?.Invoke();
    }

    public void Remove(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if(value.stackSize == 0)
            {
                inventory.Remove(value);
                m_itemDictionary.Remove(referenceData);
            }
        }

        onInventoryChangedEvent?.Invoke();
    }
}


[Serializable]
public class StartingInventoryItem
{
    public InventoryItemData itemData;
    public int amount = 10;
}
