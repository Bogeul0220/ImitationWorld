using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Creature/CreateSkill/ProjectileSkill")]
public class ProjectileSkillSO : SkillBaseSO
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed;
    [SerializeField] float startUpDelay;
    [SerializeField] int damage;
    [SerializeField] float CastTime;
    [SerializeField] float projectileRange;
    [SerializeField] float projectileHeight;
    [SerializeField] float projectileGravity;
    [SerializeField] float projectileLifeTime;
    [SerializeField] bool isHoming;

    public override IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        // 캐스트 애니메이션 실행
        caster.IsUsingSkill = true;
        yield return new WaitForSeconds(startUpDelay);
        // 캐스트 애니메이션 종료 후 발사 모션 트리거

        /*
        var createProjectile = ObjectPoolManager.Get<Projectile>(projectilePrefab);
        createProjectile.Lanched(caster, target);

        while (projectile != null && Vector3.Distance(projectile.transform.position, target.transform.position) > 0.1f)
        {
            projectile.transform.position += direction * projectileSpeed * Time.deltaTime;
            yield return null;
        }

        // 데미지 적용
        var health = target.GetComponent<Health>();
        if (health != null) health.TakeDamage(damage);
        */

        yield return new WaitForSeconds(1f);
        caster.AttackIsDone();
    }
}
