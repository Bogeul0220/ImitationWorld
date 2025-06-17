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
        unitStats.TakeDamage(damage, usePurpose);
    }
}
