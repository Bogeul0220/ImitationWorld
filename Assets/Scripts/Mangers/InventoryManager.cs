using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Dictionary<int, ItemBase> InvenItemDict = new Dictionary<int, ItemBase>(); // 아이템 데이터베이스

    public event Action OnInventoryChanged;

    private int maxInventorySize; // 최대 인벤토리 크기

    public int MaxInventorySize
    {
        get => maxInventorySize;
        set
        {
            if (value <= 0)
            {
                Debug.LogWarning("인벤토리 사이즈가 0보다 작거나 같을 수 없습니다.");
                return;
            }
            maxInventorySize = value;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (maxInventorySize <= 0)
        {
            MaxInventorySize = 24; // 기본 인벤토리 크기 설정
        }
    }

    public void AddItem(ItemBase item)
    {
        foreach (var pair in InvenItemDict)
        {
            if (pair.Value.ItemData.ItemID == item.ItemData.ItemID &&
            pair.Value is CountableItem countableItem &&
            item is CountableItem newCountableItem &&
            !countableItem.IsMax)
            {
                // 같은 아이템이 있고, CountableItem인 경우 수량을 추가
                countableItem.AddAmountAndGetExcess(newCountableItem.Amount);
                OnInventoryChanged?.Invoke();
                return; // 아이템 추가 후 종료
            }
        }

        int emptySlot = GetFirstEmptySlotIndex();
        if (emptySlot != -1)
        {
            // 빈 슬롯이 있으면 아이템 추가
            InvenItemDict[emptySlot] = item;
            OnInventoryChanged?.Invoke();
            return; // 아이템 추가 후 종료
        }
        else
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다. 아이템을 추가할 수 없습니다.");
            return; // 빈 슬롯이 없으면 종료
        }
    }

    private int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < MaxInventorySize; i++)
        {
            if (!InvenItemDict.ContainsKey(i))
                return i; // 빈 슬롯 인덱스 반환
        }
        return -1; // 빈 슬롯이 없을 경우 -1 반환
    }

    public void RemoveItem(ItemBase item, int amount = 1)
    {
        List<int> keysToRemove = new List<int>();
        int remaing = amount;

        foreach (var pair in InvenItemDict)
        {
            if (pair.Value.ItemData.ItemID != item.ItemData.ItemID)
                continue;

            if (pair.Value is CountableItem countableItem)
            {
                if (countableItem.Amount > remaing)
                {
                    countableItem.SetAmount(countableItem.Amount - remaing);
                    OnInventoryChanged?.Invoke();
                    // 아이템의 수량이 변경되었으므로, 더 이상 제거할 필요 없음
                    return;
                }
                else
                {
                    remaing -= countableItem.Amount;
                    keysToRemove.Add(pair.Key); // 나중에 제거
                }
            }
            else
            {
                keysToRemove.Add(pair.Key); // CountableItem이 아닌 경우 바로 제거
                remaing -= 1; // 수량 감소
            }

            if (remaing <= 0)
            {
                break; // 필요한 수량만큼 제거했으면 종료
            }
        }

        foreach (var key in keysToRemove)
        {
            InvenItemDict.Remove(key);
        }

        OnInventoryChanged?.Invoke();

        if (remaing > 0)
        {
            Debug.LogWarning($"인벤토리에 제거할 만큼의 {item.ItemData.ItemName}이(가) 없습니다. 부족 수량: {remaing}");
        }
    }

    public bool HasEnoughMaterials(CraftingMaterialItem materialItem)
    {
        foreach (var pair in InvenItemDict)
        {
            if (pair.Value.ItemData.ItemID == materialItem.ItemData.ItemID &&
                pair.Value is CountableItem countableItem &&
                countableItem.Amount >= materialItem.Amount)
            {
                return true; // 충분한 재료가 있음
            }
        }
        
        return false; // 충분한 재료가 없음
    }

    [ContextMenu("Debug : 인벤토리 내용 출력")]
    public void DebugPrintInventory()
    {
        Debug.Log("======인벤토리 상태 출력=======");
        foreach (var pair in InvenItemDict)
        {
            Debug.Log($"슬롯 {pair.Key}: {pair.Value.ItemData.ItemName} (수량: {(pair.Value is CountableItem countable ? countable.Amount.ToString() : "1")})");
        }
        Debug.Log($"총 아이템 수: {InvenItemDict.Count}");
        Debug.Log($"최대 인벤토리 크기: {MaxInventorySize}");
        Debug.Log("========= Done =========");
    }
}
