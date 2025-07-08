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
        yield return null;
    }
}
