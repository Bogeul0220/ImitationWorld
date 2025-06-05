using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ItemBase
{
    public ItemData ItemData;

    public ItemBase(ItemData itemData)
    {
        ItemData = itemData;
    }
}
