using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum UsePurpose
{
    None,
    FellingWood,
    MiningStone,
}

public enum MeleeWeaponType
{
    None,
    Sword,
    Axe,
}

public class MeleeWeapon : Weapon
{
    public UsePurpose usePurpose;
    public MeleeWeaponType meleeWeaponType;
    public LayerMask hitLayer;
    public BoxCollider weaponCollider;   // 무기 충돌체

    private List<Vector3> trailPositions = new List<Vector3>();
    private List<Quaternion> trailRotations = new List<Quaternion>();

    private HashSet<Collider> damagedTargets = new HashSet<Collider>();

    private float recordInterval = 0.05f;
    private float recordTimer = 0f;

    void Update()
    {
        if (!weaponCollider.enabled) return;

        // 현재 위치에 데미지
        DealDamageAt(weaponCollider.transform.position, weaponCollider.transform.rotation);

        // 위치 기록
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            trailPositions.Add(weaponCollider.transform.position);
            trailRotations.Add(weaponCollider.transform.rotation);

            // 오래된 기록 제거 (1초 전까지만 유지)
            if (trailPositions.Count > 20)
            {
                trailPositions.RemoveAt(0);
                trailRotations.RemoveAt(0);
            }
        }

        // 1초 전 위치에도 데미지
        if (trailPositions.Count > 0)
        {
            DealDamageAt(trailPositions[0], trailRotations[0]);
        }
    }

    private void DealDamageAt(Vector3 pos, Quaternion rot)
    {
        Vector3 size = weaponCollider.size;
        Vector3 center = pos + rot * weaponCollider.center;

        Collider[] hits = Physics.OverlapBox(center, size * 0.5f, rot, hitLayer);

        foreach (Collider hit in hits)
        {
            var damageable = hit.GetComponent<Damageable>();

            if (damageable != null && !damagedTargets.Contains(hit))
            {
                damageable.TakeDamage(damage, usePurpose);

                damagedTargets.Add(hit);
                _ = StartCoroutine(RemoveFromDamagedTargetAfterDelay(hit, 1f));
            }
        }
    }

    private IEnumerator RemoveFromDamagedTargetAfterDelay(Collider target, float delay)
    {
        yield return new WaitForSeconds(delay);
        damagedTargets.Remove(target);
    }

    public void StartAttack()
    {
        weaponCollider.enabled = true;
        damagedTargets.Clear();
        ClearTrail();
    }

    public void EndAttack()
    {
        weaponCollider.enabled = false;
        damagedTargets.Clear();
        ClearTrail();
    }

    private void ClearTrail()
    {
        trailPositions.Clear();
        trailRotations.Clear();
    }
}
