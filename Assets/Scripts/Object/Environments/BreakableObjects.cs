using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BreakableObjects : MonoBehaviour
{
    [SerializeField] protected UnitStats _status;

    [SerializeField] protected Vector3 savePos;

    [SerializeField] protected GameObject showPrefab;
    [SerializeField] protected Collider _collider;

    void Start()
    {
        //OnInit();
    }

    public virtual void OnInit()
    {
        if (_status == null)
            _status = GetComponent<UnitStats>();

        _status.Init();

        savePos = this.transform.position;

        _status.OnDied += OnBreak;
    }

    protected virtual void OnBreak()
    {
        showPrefab.SetActive(false);
        _collider.enabled = false;
        StartCoroutine(RespawnBreakableObject());
        _status.isDead = false;
    }

    protected IEnumerator RespawnBreakableObject()
    {
        yield return new WaitForSeconds(10f);

        ResetObject();
    }

    protected virtual void ResetObject()
    {
        showPrefab.SetActive(true);
        _collider.enabled = true;
        _status.RestoreHealth(_status.maxHealth);
    }
}
