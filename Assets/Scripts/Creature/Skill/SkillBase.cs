using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase
{
    public abstract IEnumerator OnSkill(Vector3 targetPos, int damage, GameObject projectile = null);
}
