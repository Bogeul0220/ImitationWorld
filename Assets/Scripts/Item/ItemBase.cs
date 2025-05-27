using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    public ItemData ItemData { get; private set; }

    public ItemBase(ItemData itemData)
    {
        ItemData = itemData;
    }
}
