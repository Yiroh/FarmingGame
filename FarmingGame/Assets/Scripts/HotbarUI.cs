using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


public class HotbarUI : MonoBehaviour
{
    public static HotbarUI Instance;

    [Header("Hotbar Setup")]
    public Button[] slots = new Button[9];       // assign buttons
    public Image[] slotIcons = new Image[9];     // assign icon images
    public int[] slotAmounts = new int[9];       // optional amounts
    public GameObject[] slotPrefabs = new GameObject[9]; // optional prefab references

    public int selectedHotbarIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Add button listeners first
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].onClick.AddListener(() => SelectSlot(index));
        }

        // Delay first slot selection
        Invoke(nameof(SelectFirstSlot), 0.1f);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Number keys 1–9 select hotbar slot
        for (int i = 0; i < slots.Length; i++)
        {
            Key key = Key.Digit1 + i; // Key.Digit1 to Key.Digit9
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotIcons.Length) return;

        selectedHotbarIndex = index;

        // Update colors
        for (int i = 0; i < slotIcons.Length; i++)
            if (slotIcons[i] != null)
                slotIcons[i].color = (i == index) ? Color.yellow : Color.white;

        // Send prefab to PlayerController if it exists
        if (slotPrefabs[index] != null && PlayerController.Instance != null)
        {
            PlayerController.Instance.SelectPrefab(slotPrefabs[index]);
        }
        else if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SelectPrefab(null); // clear selection
        }
    }

    public void SetQuickSlot(int index, Sprite icon, GameObject prefab = null)
    {
        if (index < 0 || index >= slots.Length) return;

        slotIcons[index].sprite = icon;
        slotIcons[index].enabled = icon != null;
        slotPrefabs[index] = prefab;
        slotAmounts[index] = 1; // optional

        // Update PlayerController if this slot is currently selected
        if (index == selectedHotbarIndex && prefab != null)
        {
            PlayerController.Instance.SelectPrefab(prefab);
        }
    }

    private void SelectFirstSlot()
    {
        if (PlayerController.Instance != null)
            SelectSlot(0);
    }
}
