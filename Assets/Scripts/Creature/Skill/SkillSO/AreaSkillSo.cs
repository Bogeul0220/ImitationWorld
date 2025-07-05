using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AreaSkill", menuName = "Creature/CreateSkill/AreaSkill")]
public class AreaSkillSO : SkillBaseSO
{
    public int Damage;
    public float DamageInterval;
    public float DamageDuration;
    public float CastTime;

    public GameObject AreaSkillPrefab;
    public AreaSkillType AreaSkillType;
    public bool ChaseTarget;

    public override IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        caster.IsUsingSkill = true;

        var areaSkill = ObjectPoolManager.Get<AreaSkillBase>(AreaSkillPrefab);
        if (areaSkill == null)
        {
            Debug.LogError("범위 공격 프리팹을 찾을 수 없습니다.");
            yield break;
        }
        var passedTime = 0f;
        while (passedTime < CastTime)
        {
            if (caster.currentState == CreatureState.TakeHit
            || caster.currentState == CreatureState.Died
            || caster.transform.localScale.x <= 0.1f
            || caster.gameObject.activeSelf == false)
            {   // 캐스터가 사망 또는 타격, 볼에 잡힌 상태일 때 발사체 반환 후 스킬 종료
                ObjectPoolManager.Return(areaSkill.gameObject);
                caster.IsUsingSkill = false;
                yield break;
            }
            passedTime += Time.deltaTime;

            switch (AreaSkillType)
            {
                case AreaSkillType.Throwing:
                    areaSkill.transform.position = caster.transform.position + caster.transform.forward * 3f + (Vector3.up * caster.navMeshAgent.height / 2f); // 캐스터의 앞에서 시작
                    break;
                case AreaSkillType.Falling:
                    areaSkill.transform.position = caster.transform.position + (Vector3.up * (caster.navMeshAgent.height + 2f)); // 캐스터 위치에서 약간 위로 시작
                    break;
                case AreaSkillType.GroundTarget:
                    areaSkill.transform.position = target.transform.position;   // 대상의 발 아래 위치에서 시작
                    break;
                case AreaSkillType.PointBlank:
                    areaSkill.transform.position = caster.transform.position; // 캐스터의 발 아래 위치에서 시작
                    break;
            }

            yield return null;
        }

        areaSkill.InitAreaSkill(caster, target, Damage, DamageInterval, DamageDuration, ChaseTarget, AreaSkillType);

        yield return new WaitUntil(() => passedTime >= CastTime);

        caster.AttackIsDone();
    }
}