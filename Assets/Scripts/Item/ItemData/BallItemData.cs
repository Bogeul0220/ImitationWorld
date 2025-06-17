using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallType
{
    Lv1,
    Lv2,
    LvMax
}

[CreateAssetMenu(fileName = "Item_Ball", menuName = "Inventory System/Item Data/Ball", order = 4)]
public class BallItemData : CountableItemData
{
    public BallType ballType;
    
    public float PercentValue => percentValue;

    // 확률
    [SerializeField] private float percentValue;

    public override ItemBase CreateItem()
    {
        return new BallItem(this);
    }
}
