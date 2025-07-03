using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAllyObject : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float throwForce;
    [SerializeField] private float upwardForce;

    private Rigidbody rb;
    private Transform targetCamera;

    private bool hasInitialized = false;

    public void Init(Transform cameraTransform)
    {
        targetCamera = cameraTransform;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 direction = (cameraTransform.position + cameraTransform.forward * 10f - transform.position).normalized;
        Vector3 force = direction * throwForce + Vector3.up * upwardForce;

        rb.velocity = force;

        hasInitialized = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasInitialized) return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            CreatureManager.Instance.SpawnAllyCreature(transform.position);
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
        ObjectPoolManager.Return(this.gameObject);
    }
}
