using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillSO", menuName = "Creature/CreateSkill")]
public class SkillSOBase : ScriptableObject
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

    public SkillBase setSkill;

    public IEnumerator StartCoolDown()
    {
        currentCooldown = setSkillCooldown;

        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            yield return null;
        }

        currentCooldown = 0f;   // 음수 시 보정
    }

    public float SkillCooldownPercent()
    {
        return currentCooldown / setSkillCooldown;
    }
}
