using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Creature/CreateSkill/ProjectileSkill")]
public class ProjectileSkillSO : SkillBaseSO
{
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public int damage;
    public float CastTime;
    public float projectileGravity;
    public float projectileLifeTime;

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
        projectile.transform.position = caster.transform.position + Vector3.up * 5f; // 캐스터 위치에서 약간 위로 시작
        projectile.transform.forward = caster.transform.forward; // 캐스터의 방향으로 설정
        var passedTime = 0f;
        while (passedTime < CastTime)
        {
            passedTime += Time.deltaTime;
            projectile.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, passedTime / CastTime);
            yield return null;
        }
        projectile.transform.localScale = Vector3.one; // 최종 크기 설정

        // 캐스트 애니메이션 종료 후 발사 모션 트리거

        projectile.InitProjectile(
            caster,
            target,
            projectileSpeed,
            damage,
            projectileGravity,
            projectileLifeTime
        );

        yield return new WaitForSeconds(0.5f);
        caster.AttackIsDone();
    }
}
