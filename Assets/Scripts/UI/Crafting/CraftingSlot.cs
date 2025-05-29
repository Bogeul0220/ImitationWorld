using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CraftingSlot : MonoBehaviour, IPointerClickHandler
{
    public CraftingUI CraftingPanel { get; set; } // 슬롯이 속한 인벤토리 UI
    [SerializeField] private Image itemIcon;

    // 슬롯에 저장될 아이템
    public ItemData CurrentItemData
    {
        get => currentItemData;
        set
        {
            currentItemData = value;

            if (currentItemData != null)
            {
                itemIcon.sprite = currentItemData.ItemIcon;
                itemIcon.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    [SerializeField] private ItemData currentItemData;

    public void InitSlot(CraftingUI craftingUI)
    {
        CraftingPanel = craftingUI;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentItemData != null)
        {
            // 아이템 클릭 시 제작 레시피 표시
            CraftingPanel.OnDisplayRecipe(CurrentItemData);
        }
    }
}
