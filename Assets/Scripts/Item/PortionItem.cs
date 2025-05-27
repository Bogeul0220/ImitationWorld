using UnityEngine;

public class PortionItem : CountableItem, IUsableItem
{
    // 수량이 있는, 사용 가능한 아이템 - 포션
    public PortionItem(PortionItemData data, int amount = 1) : base(data, amount)
    {

    }

    // 사용
    public bool Use()
    {
        Amount--;

        return true;
    }

    public override CountableItem Clone(int amount)
    {
        return new PortionItem(CountableItemData as PortionItemData, amount);
    }
}
