using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class ProjectileBase : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected float gravity;
    [SerializeField] protected float lifeTime;
    [SerializeField] protected float spinSpeed;
    protected Creature caster;
    protected UnitStats target;
    protected Vector3 direction;
    protected float timer;

    protected int damage;

    protected bool isFired = false;

    public void InitProjectile(Creature caster, UnitStats target, float speed, int damage, float gravityForce, float lifeTime, float spinSpeed)
    {
        this.caster = caster;
        this.target = target;
        this.projectileSpeed = speed;
        this.damage = damage;
        this.gravity = gravityForce;
        this.lifeTime = lifeTime;
        this.spinSpeed = spinSpeed;

        timer = 0f;

        direction = (target.transform.position + Vector3.up * 2f - transform.position).normalized;
        transform.forward = direction; // 발사체의 방향 설정

        isFired = true;

        if (gravity <= 0f)
        {
            StartCoroutine(MoveStraight()); // 중력이 없으면 직선으로 이동
        }
        else
        {
            StartCoroutine(MoveParabolic()); // 중력이 있으면 포물선으로 이동
        }
    }

    protected virtual IEnumerator MoveStraight()
    {
        while (timer < lifeTime)
        {
            if (target == null) break;

            transform.position += direction * projectileSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        ObjectPoolManager.Return(gameObject); // 수명이 다하면 오브젝트 풀로 반환
    }

    protected virtual IEnumerator MoveParabolic()
    {
        Vector3 velocity = direction * projectileSpeed;
        Vector3 pos = transform.position;

        while (timer < lifeTime)
        {
            velocity.y -= gravity * Time.deltaTime;
            pos += velocity * Time.deltaTime;
            transform.position = pos;
            transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        ObjectPoolManager.Return(gameObject); // 수명이 다하면 오브젝트 풀로 반환
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFired) return;

        Damageable damagedTarget = other.GetComponent<Damageable>();

        if (damagedTarget != null && damagedTarget != caster.Unitstat)
        {
            if (caster.AllyEnemyConversion && damagedTarget.gameObject.CompareTag("Enemy")) // 아군 크리쳐 스킬 사용 시
            {
                // 적에게 충돌 시
                damagedTarget.TakeDamage(damage, caster.Unitstat);
                ObjectPoolManager.Return(gameObject); // 오브젝트 풀로 반환
                Debug.Log($"{caster.gameObject.name}이 {damagedTarget.gameObject.name}에게 {damage}의 피해를 입혔습니다.");
            }
            else if (caster.AllyEnemyConversion == false) // 적 크리쳐 스킬 사용 시
            {
                if (damagedTarget.gameObject.CompareTag("Ally") || damagedTarget.gameObject.CompareTag("Player"))
                {
                    // 아군에게 충돌 시
                    damagedTarget.TakeDamage(damage, caster.Unitstat);
                    ObjectPoolManager.Return(gameObject); // 오브젝트 풀로 반환 
                    Debug.Log($"{caster.gameObject.name}이 {damagedTarget.gameObject.name}에게 {damage}의 피해를 입혔습니다.");
                }
            }

            // 환경물(돌, 나무)에 충돌 시
            if (other.CompareTag("Breakable"))
            {
                damagedTarget.TakeDamage(damage, caster.Unitstat);
                ObjectPoolManager.Return(gameObject); // 오브젝트 풀로 반환
            }
        }
        else if (other.CompareTag("Environment"))
        {
            // 환경 오브젝트에 충돌 시
            ObjectPoolManager.Return(gameObject); // 오브젝트 풀로 반환
        }
    }
}
