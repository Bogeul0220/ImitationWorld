using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftingMaterialItem : CountableItem
{
    public int NeedCraftCount;

    public CraftingMaterialItem(CraftingMaterialItemData data, int amount = 1) : base(data, amount)
    {

    }

    public override CountableItem Clone(int amount)
    {
        return new CraftingMaterialItem(CountableItemData as CraftingMaterialItemData, amount);
    }
}
