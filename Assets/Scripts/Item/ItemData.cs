using UnityEngine;

public enum ItemType
{
    None,
    Weapon,
    Armor,
    Consumable,
    QuestItem
}

public abstract class ItemData : ScriptableObject
{
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public string Description => description;
    public int ItemID => itemID;
    public Sprite ItemIcon => itemIcon;

    [SerializeField]
    private string itemName;
    [SerializeField]
    private ItemType itemType;
    [SerializeField]
    private string description;
    [SerializeField]
    private int itemID;
    [SerializeField]
    private Sprite itemIcon;

    public abstract ItemBase CreateItem();
}
