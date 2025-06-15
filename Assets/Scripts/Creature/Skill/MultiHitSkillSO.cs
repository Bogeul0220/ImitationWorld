using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiHitSkill", menuName = "ScriptableObjects/Skills/MultiHitSkill")]
public abstract class MultiHitSkillSO : SkillBaseSO
{
    public override IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        caster.IsUsingSkill = true;
        throw new System.NotImplementedException();
    }
}