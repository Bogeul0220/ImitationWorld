using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public InventoryUI InventoryPanel { get; set; } // 슬롯이 속한 인벤토리 UI
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemCountText;

    // 슬롯에 저장될 아이템
    public ItemBase CurrentItem
    {
        get => currentItem;
        set
        {
            currentItem = value;

            if (currentItem != null)
            {
                itemIcon.sprite = currentItem.ItemData.ItemIcon;
                itemIcon.color = new Color(1f, 1f, 1f, 1f);
                if (currentItem is CountableItem countableItem)
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
                itemIcon.sprite = null;
                itemIcon.color = new Color(1f, 1f, 1f, 0f);
                itemCountText.text = string.Empty;
            }
        }
    }
    [SerializeField] private ItemBase currentItem;

    public bool isEmpty => CurrentItem == null; // 슬롯이 비어있는지 여부

    public void InitSlot(InventoryUI inventoryUI)
    {
        InventoryPanel = inventoryUI;
        CurrentItem = null; // 슬롯 초기화
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentItem != null)
        {
            // 인벤토리 아이템 정보 패널에 아이템 정보 표시
            InventoryPanel.ShowInfoPanel(CurrentItem);
            Debug.Log($"아이템 정보 표시: {CurrentItem.ItemData.ItemName}");
        }
        else
        {
            // 아이템이 없을 경우, 아이템 정보 패널을 비움
            InventoryPanel.HideInfoPanel();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryPanel.HideInfoPanel();
    }
}
