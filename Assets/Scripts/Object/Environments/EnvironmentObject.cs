using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentObject : BreakableObjects
{
    [SerializeField] private Interact_Pickup dropObject;

    [SerializeField]private int nextDropThreshhold;

    public override void OnInit()
    {
        base.OnInit();
        nextDropThreshhold = _status.currentHealth - 10;

        _status.OnDamaged += HandleDamage;
    }

    private void HandleDamage()
    {
        while (_status.currentHealth <= nextDropThreshhold && _status.currentHealth > 0)
        {
            BranchPool(1);
            nextDropThreshhold -= 10;
        }
    }

    protected override void OnBreak()
    {
        BranchPool(3);
        base.OnBreak();
    }

    private void BranchPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var dropBranch = ObjectPoolManager.Get<Interact_Pickup>(dropObject.gameObject);
            dropBranch.transform.position = this.transform.position + Random.insideUnitSphere * 0.3f + Vector3.up * 1.5f;
        }
    }

    protected override void ResetObject()
    {
        base.ResetObject();
        nextDropThreshhold = _status.currentHealth - 10;
    }
}
