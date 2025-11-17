using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;    // Panel to show/hide
    public Inventory inventory;          // Reference to your Inventory component
    public Button[] uiButtons;           // Buttons in the grid (assign in inspector)
    public Image[] uiIcons;              // Icon images on each button (assign in inspector)
    public Text[] uiAmounts;             // Optional: text for amount (assign in inspector)

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Refresh();
        Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel.activeSelf) Close();
            else Open();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open()
    {
        inventoryPanel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        inventoryPanel.SetActive(false);
    }

    public void Refresh()
    {
        if (inventory == null) return;

        for (int i = 0; i < uiButtons.Length; i++)
        {
            Sprite icon = (i < inventory.itemIcons.Length) ? inventory.itemIcons[i] : null;
            GameObject prefab = (i < inventory.itemPrefabs.Length) ? inventory.itemPrefabs[i] : null;
            int amount = (i < inventory.itemAmounts.Length) ? inventory.itemAmounts[i] : 0;

            uiIcons[i].sprite = icon;
            uiIcons[i].enabled = icon != null;

            if (uiAmounts != null && i < uiAmounts.Length)
            {
                uiAmounts[i].text = (amount > 1) ? amount.ToString() : "";
            }

            uiButtons[i].onClick.RemoveAllListeners();
            if (icon != null)
            {
                int index = i; // closure fix
                uiButtons[i].onClick.AddListener(() =>
                {
                    HotbarUI.Instance.SetQuickSlot(HotbarUI.Instance.selectedHotbarIndex, 
                                                   inventory.itemIcons[index], 
                                                   inventory.itemPrefabs[index]);
                });
            }
        }
    }
}
