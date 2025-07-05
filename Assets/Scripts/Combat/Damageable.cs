using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public UnitStats unitStats;
    private Creature creature;
    private bool isInvincible = false;

    public void InitDamageable(UnitStats unitStats)
    {
        this.unitStats = unitStats;
        if (unitStats.GetComponent<Creature>() != null)
            creature = unitStats.GetComponent<Creature>();
    }

    public void TakeDamage(int damage, Vector3 damagedPos, UnitStats damagedTarget, UsePurpose usePurpose = UsePurpose.None)
    {
        if (unitStats.isDead || isInvincible) return;

        unitStats.TakeDamage(damage, damagedTarget, usePurpose);
        UIManager.Instance.DisplayDamageFloating(damage, transform.position + Vector3.up * 1.5f);
        EffectManager.Instance.PlayEffect(damagedPos);
        _ = StartCoroutine(DamamgedIgnoreCoroutine());
    }

    private IEnumerator DamamgedIgnoreCoroutine()
    {
        if (creature != null)
        {
            foreach (var item in creature.HitColliderList)
            {
                if (item.GetComponent<Damageable>() != null)
                    item.GetComponent<Damageable>().isInvincible = true;
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var item in creature.HitColliderList)
            {
                if (item.GetComponent<Damageable>() != null)
                    item.GetComponent<Damageable>().isInvincible = false;
            }
        }
    }
}