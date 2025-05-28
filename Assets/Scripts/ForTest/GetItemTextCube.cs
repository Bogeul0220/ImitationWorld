using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemTextCube : MonoBehaviour
{
    public PortionItemData testItemData;
    public WeaponItemData testWeaponData;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // 플레이어가 아이템을 획득했음을 알리는 메시지 출력
            Debug.Log("아이템을 획득했습니다!");

            var item = testItemData.CreateItem();
            InventoryManager.Instance.AddItem(item);

            var item2 = testWeaponData.CreateItem();
            InventoryManager.Instance.AddItem(item2);
        }
    }
}
