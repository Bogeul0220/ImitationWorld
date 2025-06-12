using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkillSO : SkillBaseSO
{
    // 평타 모션도 여기 넣을 생각중..
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed;
    [SerializeField] float startUpDelay;

    public override IEnumerator ActivateSkill(GameObject caster, GameObject target)
    {
        // 캐스트 애니메이션 실행
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
    }
}
