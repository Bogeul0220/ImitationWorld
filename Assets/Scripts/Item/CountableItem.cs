using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CountableItem : ItemBase
{
    public CountableItemData CountableItemData { get; private set; }
    public int Amount { get; protected set; }
    public int MaxAmount => CountableItemData.MaxAmount;
    public bool IsMax => Amount >= CountableItemData.MaxAmount;
    public bool IsEmpty => Amount <= 0;

    // 초기화
    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        CountableItemData = data;
        SetAmount(amount);
    }

    // 갯수 범위 제한
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    // 갯수 추가 및 초과량 반환(초과량이 없을 시 0)
    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > MaxAmount) ? nextAmount - MaxAmount : 0;
    }

    protected abstract CountableItem Clone(int amount);
}
