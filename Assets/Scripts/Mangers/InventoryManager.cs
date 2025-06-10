using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public Dictionary<int, ItemBase> InvenItemDict = new Dictionary<int, ItemBase>(); // 인벤토리 아이템 데이터베이스
    public Dictionary<int, WeaponItem> WeaponEquipDict = new Dictionary<int, WeaponItem>(); // 무기 장착 아이템 데이터베이스
    public Dictionary<EquipmentType, EquipmentItem> DefensiveEquipmentDict = new Dictionary<EquipmentType, EquipmentItem>(); // 무기 외 장착 아이템 데이터베이스

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
        if (item == null)
        {
            Debug.LogWarning("아이템이 null입니다. 아이템을 추가할 수 없습니다.");
            return; // 아이템이 null인 경우 종료
        }

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
                    remaing -= countableItem.Amount;    // 수량 감소
                    keysToRemove.Add(pair.Key);         // 이후 제거 예약
                }
            }
            else
            {
                keysToRemove.Add(pair.Key); // CountableItem이 아닌 제거 예약
                remaing -= 1; // 수량 감소
            }

            if (remaing <= 0)
            {
                break; // 필요한 수량만큼 제거했으면 종료
            }
        }

        foreach (var key in keysToRemove)
        {
            InvenItemDict.Remove(key); // 예약된 키 제거
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
                countableItem.Amount >= materialItem.NeedCraftCount)
            {
                return true; // 충분한 재료가 있음
            }
        }

        return false; // 충분한 재료가 없음
    }

    public void SwapItems(int giveSlotIndex, int takeSlotIndex)
    {
        if (giveSlotIndex == takeSlotIndex)
            return;

        ItemBase giveItem = InvenItemDict.ContainsKey(giveSlotIndex) ? InvenItemDict[giveSlotIndex] : null;
        ItemBase takeItem = InvenItemDict.ContainsKey(takeSlotIndex) ? InvenItemDict[takeSlotIndex] : null;

        if (giveItem == null)
            return;

        if (takeItem == null)
        {
            InvenItemDict[takeSlotIndex] = giveItem; // 빈 슬롯에 아이템 이동
            InvenItemDict.Remove(giveSlotIndex); // 원래 슬롯에서 아이템 제거
        }
        else if (giveItem is CountableItem giveCountable &&
                 takeItem is CountableItem takeCountable &&
                 giveCountable.ItemData.ItemID == takeCountable.ItemData.ItemID)
        {
            int excess = takeCountable.AddAmountAndGetExcess(giveCountable.Amount);
            if (excess > 0)
            {
                giveCountable.SetAmount(excess); // 기존 아이템의 수량을 초과량만큼 감소
            }
            else
            {
                InvenItemDict.Remove(giveSlotIndex); // 기존 아이템 제거
            }
        }
        else
        {
            InvenItemDict[takeSlotIndex] = giveItem; // 빈 슬롯에 아이템 이동
            InvenItemDict[giveSlotIndex] = takeItem; // 기존 아이템을 원래 슬롯으로 이동
        }

        Debug.Log($"아이템 스왑: 슬롯 {giveSlotIndex}의 {giveItem.ItemData.ItemName}과 슬롯 {takeSlotIndex}의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}을(를) 교환했습니다.");

        OnInventoryChanged?.Invoke();
    }

    public void EquipedWeapon(int giveSlotIndex, int equipSlotIndex, Slot dragSlot)
    {
        if (InvenItemDict.ContainsKey(giveSlotIndex))
            Debug.Log($"{InvenItemDict[giveSlotIndex]} 는 존재합니다.");
        else
            Debug.Log($"{InvenItemDict[giveSlotIndex]} 는 비었습니다");

        ItemBase giveItem;
        WeaponItem takeItem;

        if (dragSlot.GetComponent<EquipmentSlot>())
        {
            giveItem = WeaponEquipDict.ContainsKey(giveSlotIndex) ? WeaponEquipDict[giveSlotIndex] : null;
            takeItem = WeaponEquipDict.ContainsKey(equipSlotIndex) ? WeaponEquipDict[equipSlotIndex] : null;
        }
        else
        {
            giveItem = InvenItemDict.ContainsKey(giveSlotIndex) ? InvenItemDict[giveSlotIndex] : null;
            takeItem = WeaponEquipDict.ContainsKey(equipSlotIndex) ? WeaponEquipDict[equipSlotIndex] : null;
        }

        if (giveItem == null || !(giveItem is WeaponItem))
            return;

        if (dragSlot.GetComponent<EquipmentSlot>())
        {
            if (takeItem == null)
            {
                WeaponEquipDict[equipSlotIndex] = giveItem as WeaponItem; // 빈 슬롯에 아이템 이동
                WeaponEquipDict.Remove(giveSlotIndex); // 원래 슬롯에서 아이템 제거
            }
            else
            {
                WeaponEquipDict[equipSlotIndex] = giveItem as WeaponItem; // 빈 슬롯에 아이템 이동
                WeaponEquipDict[giveSlotIndex] = takeItem; // 기존 아이템을 원래 슬롯으로 이동
            }
        }
        else
        {
            if (takeItem == null)
            {
                WeaponEquipDict[equipSlotIndex] = giveItem as WeaponItem; // 빈 슬롯에 아이템 이동
                InvenItemDict.Remove(giveSlotIndex); // 원래 슬롯에서 아이템 제거
            }
            else
            {
                WeaponEquipDict[equipSlotIndex] = giveItem as WeaponItem; // 빈 슬롯에 아이템 이동
                InvenItemDict[giveSlotIndex] = takeItem; // 기존 아이템을 원래 슬롯으로 이동
            }
        }

        Debug.Log($"무기 장착: 슬롯 {giveSlotIndex}의 {giveItem.ItemData.ItemName}을 {equipSlotIndex}번 무기 슬롯의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}와(과) 교환했습니다.");

        OnInventoryChanged?.Invoke();
    }

    public void EquipedDefensive(int giveSlotIndex)
    {
        ItemBase giveItem = InvenItemDict.ContainsKey(giveSlotIndex) ? InvenItemDict[giveSlotIndex] : null;

        EquipmentItem asGiveItem = giveItem as EquipmentItem;

        if (giveItem == null || asGiveItem.EquipmentType == EquipmentType.Weapon)
            return;

        EquipmentItem takeItem = DefensiveEquipmentDict.ContainsKey(asGiveItem.EquipmentType) ? DefensiveEquipmentDict[asGiveItem.EquipmentType] : null;

        if (takeItem == null)
        {
            DefensiveEquipmentDict[asGiveItem.EquipmentType] = asGiveItem;
            InvenItemDict.Remove(giveSlotIndex);
        }
        else
        {
            DefensiveEquipmentDict[asGiveItem.EquipmentType] = asGiveItem;
            InvenItemDict[giveSlotIndex] = takeItem;
        }

        Debug.Log($"방어구 장착: 슬롯 {giveSlotIndex}의 {giveItem.ItemData.ItemName}을 {asGiveItem.EquipmentType.ToString()} 슬롯의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}와(과) 교환했습니다.");

        OnInventoryChanged?.Invoke();
    }

    public void UnEquipedWeapon(int unequipSlotIndex, int takeSlotIndex)
    {
        WeaponItem unequipItem = WeaponEquipDict.ContainsKey(unequipSlotIndex) ? WeaponEquipDict[unequipSlotIndex] : null;
        ItemBase takeItem = InvenItemDict.ContainsKey(takeSlotIndex) ? InvenItemDict[takeSlotIndex] : null;

        if (unequipItem == null)
            return;

        if (takeItem == null)   // 빈 슬롯으로 이동 시
        {
            InvenItemDict[takeSlotIndex] = unequipItem as WeaponItem; // 빈 슬롯에 아이템 이동
            WeaponEquipDict.Remove(unequipSlotIndex); // 원래 슬롯에서 아이템 제거
            Debug.Log($"무기 해체: 무기 슬롯 {unequipSlotIndex}의 {unequipItem.ItemData.ItemName}을 {takeSlotIndex}번 슬롯의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}와(과) 교환했습니다.");
        }
        else if (takeItem != null && takeItem is WeaponItem)
        {
            InvenItemDict[takeSlotIndex] = unequipItem as WeaponItem; // 슬롯에 아이템 이동
            WeaponEquipDict[unequipSlotIndex] = takeItem as WeaponItem; // 장비 교체
            Debug.Log($"무기 해체: 무기 슬롯 {unequipSlotIndex}의 {unequipItem.ItemData.ItemName}을 {takeSlotIndex}번 슬롯의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}와(과) 교환했습니다.");
        }
        else
        {
            InvenItemDict[GetFirstEmptySlotIndex()] = unequipItem as WeaponItem;   // 인벤토리 빈 슬롯을 찾아 아이템 이동
            WeaponEquipDict.Remove(unequipSlotIndex); // 원래 슬롯에서 아이템 제거
            Debug.Log($"무기 해체: 무기 슬롯 {unequipSlotIndex}의 {unequipItem.ItemData.ItemName}을 {GetFirstEmptySlotIndex()}번 슬롯의 {takeItem?.ItemData.ItemName ?? "빈 슬롯"}와(과) 교환했습니다.");
        }

        OnInventoryChanged?.Invoke();
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
