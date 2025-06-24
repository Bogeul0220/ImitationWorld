using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeapon : MonoBehaviour
{
    [SerializeField] private WeaponSlotInField[] weaponSlots;

    private void LateUpdate()
    {
        SetWeaponImage();
    }

    public void SetWeaponImage()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            var weaponImage = weaponSlots[i].WeaponImage;
            var weaponBorder = weaponSlots[i].WeaponBorder;
            int selectedWeaponNum = InputManager.Instance.SelectedWeaponNum;

            if (InventoryManager.Instance.WeaponEquipDict.ContainsKey(i))
            {
                weaponImage.gameObject.SetActive(true);
                weaponImage.sprite = InventoryManager.Instance.WeaponEquipDict[i].ItemData.ItemIcon;
            }
            else
            {
                weaponImage.gameObject.SetActive(false);
            }

            if (i == selectedWeaponNum)
            {
                if (InventoryManager.Instance.WeaponEquipDict.ContainsKey(selectedWeaponNum))
                {
                    if (InventoryManager.Instance.CurrentWeapon == InventoryManager.Instance.WeaponEquipDict[selectedWeaponNum])
                    {
                        weaponBorder.gameObject.SetActive(true);
                    }
                    else
                    {
                        weaponBorder.gameObject.SetActive(false);
                    }
                }

                weaponSlots[i].GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }
            else
            {
                weaponBorder.gameObject.SetActive(false);
                weaponSlots[i].GetComponent<RectTransform>().localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }
        }
    }
}
