using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum EquipmentType
{
    Weapon,
    Helm,
    Armor,
    Shield,
}

[Serializable]
public class EquipmentItem : ItemBase
{
    public EquipmentItemData EquipmentData { get; private set; }

    // 장비 타입
    public EquipmentType EquipmentType;

    public int Durability
    {
        get => durability;
        // 내구도는 0 이상, 최대 내구도 이하로 제한
        set
        {
            if (value < 0) value = 0;
            if (value > EquipmentData.MaxDurability)
                value = EquipmentData.MaxDurability;
            durability = value;
        }
    }

    private int durability;

    // 초기화
    public EquipmentItem(EquipmentItemData data) : base(data)
    {
        EquipmentData = data;
        Durability = data.MaxDurability;
    }
}
