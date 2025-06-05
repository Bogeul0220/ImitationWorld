using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusSO", menuName = "Unit/StatusSO", order = 1)]
public class StatusSO : ScriptableObject
{
    public int MaxHp;
    public int MaxStamina;

    public StatusSO DeepCopy()
    {
        StatusSO newStatusSO = new StatusSO();
        newStatusSO.MaxHp = this.MaxHp;
        newStatusSO.MaxStamina = this.MaxStamina;

        return newStatusSO;
    }
}
