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

    public void TakeDamage(int damage, UsePurpose usePurpose = UsePurpose.None)
    {
        if(unitStats.isDead) return;
        
        unitStats.TakeDamage(damage, usePurpose);
        UIManager.Instance.DisplayDamageFloating(damage, transform.position + Vector3.up * 1.5f);
    }
}
