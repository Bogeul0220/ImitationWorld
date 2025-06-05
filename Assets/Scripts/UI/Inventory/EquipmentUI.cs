using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField]
    private EquipmentSlot[] weaponSlots = new EquipmentSlot[4];
    [SerializeField]
    private EquipmentSlot helmSlot;
    [SerializeField]
    private EquipmentSlot armorSlot;
    [SerializeField]
    private EquipmentSlot shieldSlot;

    void Awake()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].InitSlot(i);
        }
        helmSlot.InitSlot();
        armorSlot.InitSlot();
        shieldSlot.InitSlot();
    }

    void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += UpdateEquipmentUI;
        UpdateEquipmentUI();
    }

    public void UpdateEquipmentUI()
    {
        var weaponDict = InventoryManager.Instance.WeaponEquipDict;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponDict.ContainsKey(i))
            {
                weaponSlots[i].CurrentItem = weaponDict[i];
            }
            else
            {
                weaponSlots[i].CurrentItem = null;
            }
        }

        var defenceDict = InventoryManager.Instance.DefensiveEquipmentDict;

        if (defenceDict.ContainsKey(EquipmentType.Helm))
            helmSlot.CurrentItem = defenceDict[EquipmentType.Helm];
        else
            helmSlot.CurrentItem = null;

        if (defenceDict.ContainsKey(EquipmentType.Armor))
            armorSlot.CurrentItem = defenceDict[EquipmentType.Armor];
        else
            armorSlot.CurrentItem = null;

        if (defenceDict.ContainsKey(EquipmentType.Shield))
            shieldSlot.CurrentItem = defenceDict[EquipmentType.Shield];
        else
            shieldSlot.CurrentItem = null;
    }
}
