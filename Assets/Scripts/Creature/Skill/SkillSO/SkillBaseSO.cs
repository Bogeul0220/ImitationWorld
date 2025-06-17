using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class SkillBaseSO : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public string skillDescription;
    public float setSkillCooldown;

    public virtual IEnumerator ActivateSkill(Creature caster, UnitStats target)
    {
        yield return null; // 기본 구현은 아무것도 하지 않음
    }
}
