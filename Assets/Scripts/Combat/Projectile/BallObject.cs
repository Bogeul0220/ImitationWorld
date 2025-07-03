using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : MonoBehaviour
{
    public int MaxBounce = 3;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float throwForce;
    [SerializeField] private float upwardForce;

    private Rigidbody rb;
    private int bounceCount = 0;
    private Transform targetCamera;

    private bool hasInitialized = false;

    public void Init(Transform cameraTransform)
    {
        targetCamera = cameraTransform;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 spawnOffset = cameraTransform.right * 0.5f + cameraTransform.up * 0.5f;
        //transform.position = cameraTransform.position + spawnOffset;

        Vector3 direction = (cameraTransform.position + cameraTransform.forward * 10f - transform.position).normalized;
        Vector3 force = direction * throwForce + Vector3.up * upwardForce;

        rb.velocity = force;

        hasInitialized = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasInitialized) return;

        if (other.CompareTag("Enemy"))
        {
            // 포획 함수 실행
            hasInitialized = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            Creature targetCreature = other.GetComponent<Catchable>().CatchCreture;
            if (targetCreature != null)
            {
                StartCoroutine(HandleCaptureSequence(targetCreature));
            }
        }
        else
            return;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasInitialized) return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            bounceCount++;

            Vector3 currentVelocity = rb.velocity;
            Vector3 bounceDir = Vector3.ProjectOnPlane(currentVelocity, collision.contacts[0].normal).normalized;
            Vector3 bounceForce = (bounceDir * throwForce * 0.5f) + (Vector3.up * upwardForce * 0.5f);

            rb.velocity = Vector3.zero; // 기존 속도 초기화
            rb.AddForce(bounceForce, ForceMode.VelocityChange); //즉시 튕겨나감

            if (bounceCount >= MaxBounce)
            {
                ReturnObject();
            }
        }
    }

    private IEnumerator HandleCaptureSequence(Creature target)
    {
        float totalScore = 0;

        // 1. 기본 점수 (볼 종류별)
        switch (InputManager.Instance.SelectedBallIndex)
        {
            case 0: // 일반 볼
                totalScore += 30f;
                break;
            case 1: // 고급 볼
                totalScore += 45f;
                break;
            case 2: // 마스터 볼
                totalScore += 60f;
                break;
        }

        // 2. 체력 기반 보너스 (최대 30점)
        float healthPercentage = target.CreatureStat.maxHealth > 0 ? 
            target.CreatureStat.currentHealth / target.CreatureStat.maxHealth : 1f;
        float healthBonus = (1f - healthPercentage) * 30f;
        totalScore += healthBonus;

        yield return StartCoroutine(target.CreatureSizeDown());

        float initialRand = Random.Range(0f, 100f);
        if (initialRand <= totalScore)
        {
            CaptureSuccess(target);
            yield break;
        }

        for (int i = 0; i < 3; i++)
        {
            float shakeDuration = 1f;
            float shakeMagnitude = 15f;
            float elapsed = 0f;
            Quaternion originalRot = transform.rotation;

            while (elapsed < shakeDuration)
            {
                float zAngle = Mathf.Sin(elapsed * 10f) * shakeMagnitude;
                transform.rotation = originalRot * Quaternion.Euler(0f, 0f, zAngle);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.rotation = originalRot;

            if (Random.value <= 0.5f)
            {
                float bonus = Random.Range(1f, 20f);
                totalScore += bonus;
            }

            if (totalScore >= 100)
            {
                CaptureSuccess(target);
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        if (totalScore >= 100)
        {
            CaptureSuccess(target);
            yield break;
        }
        else
        {
            yield return target.CreatrueSizeUp(transform.position);
            ReturnObject();
        }
    }

    private void CaptureSuccess(Creature target)
    {   
        if (CreatureManager.Instance.SpawnedWildCreatures.Contains(target))
            CreatureManager.Instance.SpawnedWildCreatures.Remove(target);

        // 중복 체크 후 안전하게 추가
        if (!CreatureManager.Instance.SpawnedTamedKey.Contains(target.CreatureIndex) && !CreatureManager.Instance.TamedCreatures.ContainsKey(target.CreatureIndex))
        {
            CreatureManager.Instance.SpawnedTamedKey.Add(target.CreatureIndex);
            CreatureManager.Instance.TamedCreatures.Add(target.CreatureIndex, target);
        }
        else
        {
            int newIndex = -1;
            while(true)
            {
                newIndex = (int)UnityEngine.Random.Range(1000000000, 10000000000);
                if(newIndex != target.CreatureIndex)
                {
                    target.CreatureIndex = newIndex;
                    CreatureManager.Instance.SpawnedTamedKey.Add(target.CreatureIndex);
                    CreatureManager.Instance.TamedCreatures.Add(target.CreatureIndex, target);
                    break;
                }
            }
        }
        
        target.HitColliderList.Clear();
        target.gameObject.SetActive(false);
        ReturnObject();
    }

    void ReturnObject()
    {
        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        hasInitialized = false;
        bounceCount = 0;
        ObjectPoolManager.Return(this.gameObject);
    }
}
