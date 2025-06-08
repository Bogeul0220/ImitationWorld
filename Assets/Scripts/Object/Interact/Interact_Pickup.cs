using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact_Pickup : InteractionObjectBase
{
    [SerializeField] private ItemData itemData;

    public override void OnInteract()
    {
        var newItem = itemData.CreateItem();

        InventoryManager.Instance.AddItem(newItem);

        ObjectPoolManager.Return(this.gameObject);
    }
}
