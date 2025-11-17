using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Inventory Setup")]
    public Button[] inventoryButtons;      // assign in inspector
    public Image[] inventoryIcons;         // assign in inspector
    public int[] itemAmounts;              // optional amounts
    public Sprite[] itemIcons;             // assign icon per slot
    public GameObject[] itemPrefabs;       // optional prefab reference per slot

    // Add or replace item at a slot
    public void SetItem(int slotIndex, Sprite icon, GameObject prefab = null)
    {
        if (slotIndex < 0 || slotIndex >= inventoryButtons.Length) return;

        inventoryIcons[slotIndex].sprite = icon;
        inventoryIcons[slotIndex].enabled = icon != null;

        itemPrefabs[slotIndex] = prefab;
        itemAmounts[slotIndex] = 1;

        inventoryButtons[slotIndex].onClick.RemoveAllListeners();
        if (icon != null)
        {
            int index = slotIndex; // closure fix
            inventoryButtons[slotIndex].onClick.AddListener(() =>
            {
                HotbarUI.Instance.SetQuickSlot(
                    HotbarUI.Instance.selectedHotbarIndex,
                    itemIcons[index],  // <- correct here
                    itemPrefabs[index]
                );
            });
        }
    }

    // Consume 1 from slot
    public bool ConsumeItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= itemAmounts.Length) return false;
        if (itemAmounts[slotIndex] > 0)
        {
            itemAmounts[slotIndex]--;
            return true;
        }
        return false;
    }
}
