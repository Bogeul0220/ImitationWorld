using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemInfoPanel : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemNameText, itemDescriptionText, itemInfoText, itemPriceText;

    public void ShowItemInfo(ItemBase item)
    {
        var itemData = item.ItemData;
        itemIconImage.sprite = itemData.ItemIcon;
        itemIconImage.color = new Color(1f, 1f, 1f, 1f);
        itemNameText.text = itemData.ItemName;
        itemDescriptionText.text = itemData.Description;
        if(item is EquipmentItem equipmentItem)
        {
            itemInfoText.text = $"내구도 : {equipmentItem.EquipmentData.MaxDurability}";
        }
        else if(item is CountableItem countableItem)
        {
            itemInfoText.text = $"개수 : {countableItem.Amount}";
        }
        else
        {
            itemInfoText.text = "-";
        }
    }
}
