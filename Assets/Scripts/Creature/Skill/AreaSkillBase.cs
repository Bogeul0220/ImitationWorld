using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AreaSkillType
{
    Throwing,
    Falling,
    GroundTarget,
    PointBlank,
}

public class AreaSkillBase : MonoBehaviour
{
    private Creature caster;
    private UnitStats target;
    private int damage;
    private float damageInterval;
    private float damageDuration;
    private bool chaseTarget;

    [SerializeField] private AreaSkillParticleHandler areaSkillParticleHandler;

    public void InitAreaSkill(Creature caster, UnitStats target, int damage, float damageInterval,
     float damageDuration, bool chaseTarget, AreaSkillType _areaSkillType)
    {
        this.caster = caster;
        this.target = target;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.damageDuration = damageDuration;
        this.chaseTarget = chaseTarget;

        switch (_areaSkillType)
        {
            case AreaSkillType.Throwing:
                StartCoroutine(AreaThrowing());
                break;
            case AreaSkillType.Falling:
                StartCoroutine(AreaFallingDamage());
                break;
            case AreaSkillType.GroundTarget:
                StartCoroutine(AreaGroundTargetDamage());
                break;
            case AreaSkillType.PointBlank:
                StartCoroutine(AreaPointBlankDamage());
                break;
        }
    }

    private IEnumerator AreaThrowing()
    {
        areaSkillParticleHandler.InitAreaSkillParticleHandler(damage, caster, damageInterval);
        yield return null;
    }

    private IEnumerator AreaFallingDamage()
    {
        areaSkillParticleHandler.InitAreaSkillParticleHandler(damage, caster, damageInterval);

        float moveDuration = 3f;
        float scaleDuration = 1f;
        Vector3 startPos = transform.position;
        Vector3 endPos = chaseTarget ? target.transform.position + Vector3.up * 6f : target.transform.position + Vector3.up * 10f;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t / moveDuration);
            yield return null;
        }

        transform.position = endPos;

        if (!chaseTarget)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * 2f;

            float scaleT = 0f;
            while (scaleT < scaleDuration)
            {
                scaleT += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, scaleT / scaleDuration);
                yield return null;
            }
        }

        if (areaSkillParticleHandler.gameObject.activeSelf == false)
            areaSkillParticleHandler.gameObject.SetActive(true);

        areaSkillParticleHandler.EnableParticle();

        float damageT = 0f;
        while (damageT < damageDuration)
        {
            damageT += Time.deltaTime;

            if (chaseTarget && target != null)
                transform.position = target.transform.position + Vector3.up * 5f;
            else
                transform.position = endPos;

            yield return null;
        }

        areaSkillParticleHandler.DisableParticle();
        ObjectPoolManager.Return(gameObject);

        yield return null;
    }

    private IEnumerator AreaGroundTargetDamage()
    {
        yield return null;
    }

    private IEnumerator AreaPointBlankDamage()
    {
        yield return null;
    }
}