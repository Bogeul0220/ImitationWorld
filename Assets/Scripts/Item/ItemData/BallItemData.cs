using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallType
{
    Normal,
    Advanced,
    Master
}

[CreateAssetMenu(fileName = "Item_Ball", menuName = "Inventory System/Item Data/Ball", order = 4)]
public class BallItemData : CountableItemData
{
    public BallType ballType;

    public override ItemBase CreateItem()
    {
        return new BallItem(this);
    }
}
