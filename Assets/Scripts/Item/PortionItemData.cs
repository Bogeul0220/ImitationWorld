using UnityEngine;

[CreateAssetMenu(fileName = "Item_Portion", menuName = "Inventory System/Item Data/Portion", order = 3)]
public class PortionItemData : CountableItemData
{
    public float Value => value;

    // 회복량
    [SerializeField] private float value;

    public override ItemBase CreateItem()
    {
        return new PortionItem(this);
    }
}
