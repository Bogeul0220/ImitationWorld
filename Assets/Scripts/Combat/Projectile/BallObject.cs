using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : MonoBehaviour
{
    public int MaxBounce = 3;
    public float CatchValue;
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

        switch (InputManager.Instance.SelectedBallIndex)
        {
            case 0:
                CatchValue = 50;
                break;
            case 1:
                CatchValue = 75;
                break;
            case 2:
                CatchValue = 100;
                break;
        }

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
        yield return StartCoroutine(target.CatchCreature());

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

        float rand = Random.Range(0f, 100f);
        bool isSuccess = rand <= CatchValue;

        if (isSuccess)
        {
            if (CreatureManager.Instance.SpawnedWildCreatures.Contains(target))
                CreatureManager.Instance.SpawnedWildCreatures.Remove(target);

            CreatureManager.Instance.SpawnedTamedCreatures.Add(target);
            target.gameObject.SetActive(false);
            ReturnObject();
        }
        else
        {
            yield return target.CapturedFail();
            ReturnObject();
        }
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
