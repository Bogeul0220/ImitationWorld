using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public UnitStats unitStats;

    public void InitDamageable(UnitStats unitStats)
    {
        this.unitStats = unitStats;
    }

    public void TakeDamage(int damage, UnitStats damagedTarget, UsePurpose usePurpose = UsePurpose.None)
    {
        if(unitStats.isDead) return;
        
        unitStats.TakeDamage(damage, damagedTarget, usePurpose);
        UIManager.Instance.DisplayDamageFloating(damage, transform.position + Vector3.up * 1.5f);
    }
}
