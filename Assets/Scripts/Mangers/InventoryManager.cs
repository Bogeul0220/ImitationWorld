using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public List<ItemBase> CurrentItemList = new List<ItemBase>(24);

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void AddItem(ItemBase item)
    {
        foreach (var existingItem in CurrentItemList)
        {
            if (existingItem.ItemData.ItemID == item.ItemData.ItemID)
            {
                if (existingItem is CountableItem countableItem &&
                    item is CountableItem newCountableItem &&
                    !countableItem.IsMax)
                {
                    int excess = countableItem.AddAmountAndGetExcess(newCountableItem.Amount);
                    if (excess > 0)
                    {
                        var clonedItem = newCountableItem.Clone(excess);
                        AddItem(clonedItem);
                    }
                }
                return;
            }
        }

        if (CurrentItemList.Count < 24)
        {
            CurrentItemList.Add(item);
        }
        else
        {
            Debug.LogWarning("Inventory is full! Cannot add more items.");
        }

        OnInventoryChanged?.Invoke();
    }
    
    public void RemoveItem(ItemBase item, int amount = 1)
    {
        if (CurrentItemList.Contains(item))
        {
            if (item is CountableItem countableItem)
            {
                countableItem.SetAmount(countableItem.Amount - amount);
                if (countableItem.IsEmpty)
                {
                    CurrentItemList.Remove(item);
                }
            }
            else
            {
                CurrentItemList.Remove(item);
            }
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("Item not found in inventory!");
        }
    }
}
