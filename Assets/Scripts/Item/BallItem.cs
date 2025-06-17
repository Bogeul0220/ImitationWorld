using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BallItem : CountableItem
{
    public BallItem(CountableItemData data, int amount = 1) : base(data, amount)
    {
    }

    public override CountableItem Clone(int amount)
    {
        return new BallItem(CountableItemData as BallItemData, amount);
    }
}
