using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AreaSkill", menuName = "ScriptableObjects/Skills/AreaSkill")]
public abstract class AreaSkillSO : SkillBaseSO
{
    [SerializeField] float radius;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject areaEffectPrefab;

    public override IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        caster.IsUsingSkill = true;
        // 이펙트 표시
        /*
        GameObject effect = Instantiate(areaEffectPrefab, target.transform.position, Quaternion.identity);
        Destroy(effect, 2f);

        yield return new WaitForSeconds(0.5f); // 준비 시간

        // 범위 내 적 찾기
        Collider[] hitTargets = Physics.OverlapSphere(target.transform.position, radius, targetLayer);
        foreach (var hit in hitTargets)
        {
            var health = hit.GetComponent<Health>();
            if (health != null) health.TakeDamage(damage);
        }
        */
        yield return null;
    }
}