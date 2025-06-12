using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillSO", menuName = "Creature/CreateSkill")]
public abstract class SkillBaseSO : ScriptableObject
{
    [SerializeField] string skillName;
    [SerializeField] Sprite skillIcon;
    [SerializeField] int damamge;
    [SerializeField] string skillDescription;
    [SerializeField] float currentCooldown;
    [SerializeField] float setSkillCooldown;

    public bool Useable
    {
        get
        {
            if (currentCooldown <= 0f)
                return true;

            return false;
        }
    }

    public virtual IEnumerator ActivateSkill()
    {
        yield return null;
    }

    public IEnumerator StartCoolDown()
    {
        currentCooldown = setSkillCooldown;

        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            yield return null;
        }

        currentCooldown = 0f;   // 음수 시 리셋
    }

    public float SkillCooldownPercent() => currentCooldown / setSkillCooldown;

    public abstract IEnumerator ActivateSkill(GameObject caster, GameObject target);
}
