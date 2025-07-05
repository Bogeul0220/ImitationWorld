using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSkillParticleHandler : MonoBehaviour
{
    public int Damage = 10;
    public Creature Caster; // 데미지 출처
    public float DamageInterval;

    private ParticleSystem particle;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private bool damageFlag = false;

    public void InitAreaSkillParticleHandler(int damage, Creature caster, float damageInterval)
    {
        Damage = damage;
        Caster = caster;
        DamageInterval = damageInterval;
        particle = GetComponent<ParticleSystem>();

        if(particle != null)
        {
            var collision = particle.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            
            if(caster.AllyEnemyConversion)
            {
                collision.collidesWith = LayerMask.GetMask("Enemy");
            }
            else
            {
                collision.collidesWith = LayerMask.GetMask("Ally", "Player");
            }
        }
    }

    // 파티클이 무언가와 충돌할 때 호출됨
    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 collisionPos = collisionEvents[i].intersection;
            Damageable target = other.GetComponent<Damageable>();

            if (target != null)
            {
                if (damageFlag == true) return;

                target.TakeDamage(Damage, collisionPos, Caster != null ? Caster.CreatureStat : null);
                _ = StartCoroutine(AreaSkillParticleHandlerCoroutine());
            }
        }
    }

    private IEnumerator AreaSkillParticleHandlerCoroutine()
    {
        damageFlag = true;
        yield return new WaitForSeconds(DamageInterval);
        damageFlag = false;
    }

    public void EnableParticle()
    {
        if (particle == null) return;

        particle.Play();
    }

    public void DisableParticle()
    {
        if (particle == null) return;

        particle.Stop();
    }
}