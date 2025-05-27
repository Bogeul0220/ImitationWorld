using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    // 슬롯에 저장될 아이템
    public ItemBase CurrentItem
    {
        get => currentItem;
        private set
        {
            if (currentItem != null)
            {
                itemIcon = currentItem.ItemData.ItemIcon;
                itemDescriptionText.text = currentItem.ItemData.Description;
                if(currentItem is CountableItem countableItem)
                {
                    itemCountText.text = countableItem.Amount.ToString();
                }
                else
                {
                    itemCountText.text = string.Empty;
                }
            }
            else
            {
                itemIcon = null;
                itemCountText.text = string.Empty;
                itemDescriptionText.text = string.Empty;
            }
        }
    }
    [SerializeField] private ItemBase currentItem;

    public bool isEmpty => CurrentItem == null; // 슬롯이 비어있는지 여부

    [SerializeField] private Sprite itemIcon;
    [SerializeField] private TMP_Text itemCountText;
    [SerializeField] private TMP_Text itemDescriptionText;
}
