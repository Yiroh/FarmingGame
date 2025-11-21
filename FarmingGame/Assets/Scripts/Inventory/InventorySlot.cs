using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image m_icon;
    [SerializeField] private TextMeshProUGUI m_label;
    [SerializeField] private GameObject m_stackObj;
    [SerializeField] private TextMeshProUGUI m_stackLabel;

    private InventoryItem item;

    public void Set(InventoryItem newItem)
    {
        item = newItem;
        m_icon.sprite = item.data.icon;
        m_label.text = item.data.displayName;

        if (item.stackSize <= 1)
        {
            m_stackObj.SetActive(false);
        }
        else
        {
            m_stackObj.SetActive(true); // make sure it's visible
            m_stackLabel.text = item.stackSize.ToString();
        }
    }

    public void OnClick()
    {
        if (item != null && item.data.prefab != null)
        {
            Debug.Log("Selected Prefab");
            PlayerController.Instance.SelectPrefab(item.data.prefab);
            PlayerController.Instance.currentAction = GridAction.Place;
        }
    }
}
