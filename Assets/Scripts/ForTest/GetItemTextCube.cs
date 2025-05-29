using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemTextCube : MonoBehaviour
{
    public List<ItemData> itemDatas;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (var itemData in itemDatas)
            {
                var newItem = itemData.CreateItem();
                InventoryManager.Instance.AddItem(newItem);
            }
        }
    }
}
