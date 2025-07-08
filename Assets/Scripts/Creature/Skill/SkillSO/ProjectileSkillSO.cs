using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Creature/CreateSkill/ProjectileSkill")]
public class ProjectileSkillSO : SkillBaseSO
{
    public GameObject projectilePrefab;
    public float projectileSpinSpeed;
    public float projectileSpeed;
    public int damage;
    public float CastTime;
    public float projectileGravity;
    public float projectileLifeTime;
    public bool StartCasterFront;

    public override IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        // 캐스트 애니메이션 실행
        caster.IsUsingSkill = true;
        var projectile = ObjectPoolManager.Get<ProjectileBase>(projectilePrefab);
        if (projectile == null)
        {
            Debug.LogError("발사체 프리팹을 찾을 수 없습니다.");
            yield break;
        }
        projectile.transform.localScale = Vector3.zero;
        projectile.transform.forward = caster.transform.forward; // 캐스터의 방향으로 설정
        float passedTime = 0f;
        while (passedTime < CastTime)
        {
            if (caster.currentState == CreatureState.TakeHit
            || caster.currentState == CreatureState.Died
            || caster.transform.localScale.x <= 0.1f
            || caster.gameObject.activeSelf == false)
            {   // 캐스터가 사망 또는 타격, 볼에 잡힌 상태일 때 발사체 반환 후 스킬 종료
                ObjectPoolManager.Return(projectile.gameObject);
                caster.IsUsingSkill = false;
                yield break;
            }
            passedTime += Time.deltaTime;
            projectile.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, passedTime / CastTime);
            if (StartCasterFront)
            {
                projectile.transform.position = caster.transform.position + caster.transform.forward * 3f + (Vector3.up * caster.navMeshAgent.height / 2f); // 캐스터의 앞에서 시작
            }
            else
            {
                projectile.transform.position = caster.transform.position + (Vector3.up * (caster.navMeshAgent.height + 2f)); // 캐스터 위치에서 약간 위로 시작
            }
            projectile.transform.rotation = caster.transform.rotation;
            yield return null;
        }
        projectile.transform.localScale = Vector3.one; // 최종 크기 설정

        projectile.InitProjectile(
            caster,
            target,
            projectileSpeed,
            damage,
            projectileGravity,
            projectileLifeTime,
            projectileSpinSpeed
        );

        SoundManager.Instance.PlaySFX($"{skillName}");    

        yield return new WaitUntil(() => passedTime >= CastTime);
        
        caster.AttackIsDone();
    }
}
