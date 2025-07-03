using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class AreaSkillBase : MonoBehaviour
{
    private Creature caster;
    private UnitStats target;
    private float damage;
    private float damageInterval;
    private float damageDuration;
    private int damageCount;
    private bool chaseTarget;

    public void InitAreaSkill(Creature caster, UnitStats target, float damage, float damageInterval, float damageDuration, int damageCount, bool startCasterFront, bool chaseTarget)
    {
        this.caster = caster;
        this.target = target;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.damageDuration = damageDuration;
        this.damageCount = damageCount;
        this.chaseTarget = chaseTarget;
        if (startCasterFront)
        {
            StartCoroutine(AreaThrowing());
        }
        else
        {
            StartCoroutine(AreaFallingDamage());
        }
    }

    private IEnumerator AreaThrowing()
    {
        yield return null;
    }

    private IEnumerator AreaFallingDamage()
    {
        yield return null;
    }
}