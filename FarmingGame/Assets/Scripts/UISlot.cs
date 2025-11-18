using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UISlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;
    public Button button;

    [HideInInspector] public int index;

    public void Set(Sprite sprite, int amount)
    {
        icon.sprite = sprite;
        icon.enabled = sprite != null;

        amountText.text = (amount > 1) ? amount.ToString() : "";
    }
}
