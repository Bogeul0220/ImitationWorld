using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

public class EquipmentSlot : Slot
{
    public EquipmentType EquipmentType;
    public override ItemBase CurrentItem
    {
        get => currentItem;
        set
        {
            currentItem = value;

            if (currentItem != null)
            {
                itemIcon.sprite = currentItem.ItemData.ItemIcon;
                itemIcon.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    public void InitSlot(int _slotIndex = 0)
    {
        SlotIndex = _slotIndex;
        CurrentItem = null;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this || draggedSlot.isEmpty || !(draggedSlot.CurrentItem is EquipmentItem))
            return;
        // 드래그 중인 슬롯이 없으면 무시, 장착 아이템이 아니면 무시

        // 인벤토리 아이템 장착 후 딕셔너리 업데이트
        if (EquipmentType == EquipmentType.Weapon)
        {
            InventoryManager.Instance.EquipedWeapon(draggedSlot.SlotIndex, this.SlotIndex);
        }
        else
        {
            InventoryManager.Instance.EquipedDefensive(draggedSlot.SlotIndex);
        }
    }
}
