using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public GameObject slotPrefab;
    public Transform container;
    public Inventory inventory;

    private List<UISlot> uiSlots = new List<UISlot>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateSlots(inventory.slots.Count);
        Refresh();
        Close();
    }

    void GenerateSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, container);
            UISlot u = obj.GetComponent<UISlot>();

            u.index = i;

            // Clicking assigns to hotbar
            u.button.onClick.AddListener(() =>
            {
                Inventory.InventorySlot slot = inventory.slots[u.index];
                if (!slot.IsEmpty)
                {
                    // TODO: Need to remove the inventory's slot icon and slot.prefab when we assign to hotbar, if successful.
                    HotbarUI.Instance.SetQuickSlotAuto(slot.icon, slot.prefab);
                }
            });

            uiSlots.Add(u);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            var slot = inventory.slots[i];
            uiSlots[i].Set(slot.icon, slot.amount);
        }
    }

    public void Open() => container.gameObject.SetActive(true);
    public void Close() => container.gameObject.SetActive(false);
}
