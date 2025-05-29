using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_Crafting", menuName = "Inventory System/Item Data/CraftingMaterial", order = 4)]
public class CraftingMaterialItemData : CountableItemData
{


    public override ItemBase CreateItem()
    {
        return new CraftingMaterialItem(this);
    }
}
