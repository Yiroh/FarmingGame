using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    public static HotbarUI Instance;

    public GameObject slotPrefab;
    public Transform hotbarContainer;

    private List<UISlot> slots = new List<UISlot>();
    public int selectedHotbarIndex = 0;
    private GameObject[] slotPrefabs;

    private void Awake()
    {
        Instance = this;
        slotPrefabs = new GameObject[9];
    }

    private void Start()
    {
        GenerateSlots(9);
        SelectSlot(0); // select first hotbar slot by default
    }

    private void Update()
    {
        HandleHotbarKeyInput();
    }

    private void HandleHotbarKeyInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectSlot(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectSlot(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SelectSlot(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectSlot(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) SelectSlot(8);
    }

    private void GenerateSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, hotbarContainer);
            UISlot ui = obj.GetComponent<UISlot>();
            ui.index = i;

            // When clicking a hotbar slot, select it for building
            ui.button.onClick.AddListener(() => SelectSlot(ui.index));

            slots.Add(ui);
        }
    }

    public void SetQuickSlot(int index, Sprite icon, GameObject prefab)
    {
        if (index < 0 || index >= slots.Count) return;

        slots[index].Set(icon, 1);        // Update button image
        slotPrefabs[index] = prefab;      // Store prefab

        // If this slot is currently selected, tell player
        if (index == selectedHotbarIndex)
            PlayerController.Instance?.SelectPrefab(prefab);
    }

    public void SetQuickSlotAuto(Sprite icon, GameObject prefab)
    {
        // First empty slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (slotPrefabs[i] == null)
            {
                SetQuickSlot(i, icon, prefab);
                return;
            }
        }
        // If all full, overwrite currently selected
        SetQuickSlot(selectedHotbarIndex, icon, prefab);
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        selectedHotbarIndex = index;

        // Highlight selected slot
        for (int i = 0; i < slots.Count; i++)
            slots[i].icon.color = (i == index ? Color.yellow : Color.white);

        // Tell player which prefab to use
        PlayerController.Instance?.SelectPrefab(slotPrefabs[index]);
        Debug.Log($"Selected slot {index+1} with prefab {slotPrefabs[index]}");
    }
}
