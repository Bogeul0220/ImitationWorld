using UnityEngine;

[CreateAssetMenu(fileName = "Item_Weapon", menuName = "Inventory System/Item Data/Weapon", order = 1)]
public class WeaponItemData : EquipmentItemData
{
    private int Damage => damage;
    
    [SerializeField] private int damage = 1;

    public override ItemBase CreateItem()
    {
        return new WeaponItem(this);
    }
}
