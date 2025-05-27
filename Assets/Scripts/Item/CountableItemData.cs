using UnityEngine;

public abstract class CountableItemData : ItemData
{
    public int MaxAmount => maxAmount;

    [SerializeField]
    private int maxAmount = 99;
}
