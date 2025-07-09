using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public StatusSO statusSO;
    public int currentHealth;
    public int maxHealth;
    public int currentStamina;
    public int maxStamina;
    public bool isDead;

    public event Action OnDamaged;
    public event Action OnDied;
    public UnitStats CurrentBattleTarget;
    public Dictionary<UnitStats, int> DamagedTargetDict = new Dictionary<UnitStats, int>();

    public UsePurpose usePurpose;

    void Update()
    {
        if(DamagedTargetDict.Count > 0)
        {
            List<UnitStats> damagedTargetList = new List<UnitStats>();
            if(DamagedTargetDict.Count > 0)
            {
                foreach(var item in DamagedTargetDict)
                {
                    if(item.Key.isDead || Vector3.Distance(item.Key.transform.position, transform.position) > 25f || item.Key.gameObject.activeInHierarchy == false)
                    {
                        damagedTargetList.Add(item.Key);
                    }
                }
            }

            foreach(var item in damagedTargetList)
            {
                if(CurrentBattleTarget == item)
                    CurrentBattleTarget = null;
                    
                DamagedTargetDict.Remove(item);
            }
        }
    }

    public void Init()
    {
        isDead = false;

        if (statusSO != null)
        {
            maxHealth = statusSO.MaxHp;
            maxStamina = statusSO.MaxStamina;

            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }
    }

    public void TakeDamage(int damage, UnitStats damagedTarget, UsePurpose fromUsePurpose = UsePurpose.None)
    {
        if (isDead) return;

        if (DamagedTargetDict.ContainsKey(damagedTarget))
        {
            DamagedTargetDict[damagedTarget] += damage;
        }
        else
        {
            DamagedTargetDict.Add(damagedTarget, damage);
        }

        for (int i = 0; i < DamagedTargetDict.Count; i++)
        {
            if (CurrentBattleTarget == null)
            {
                CurrentBattleTarget = DamagedTargetDict.Keys.ElementAt(i);
            }
            else if (DamagedTargetDict.Values.ElementAt(i) > DamagedTargetDict[CurrentBattleTarget])
            {
                CurrentBattleTarget = DamagedTargetDict.Keys.ElementAt(i);
            }
        }

        if (this.usePurpose == fromUsePurpose)
        {
            damage *= 2;
            Debug.Log("Double Damage");
        }

        UIManager.Instance.DisplayDamageFloating(damage, transform.position + Vector3.up * 1.5f);
        currentHealth -= damage;

        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            if (CurrentBattleTarget != null)
                CurrentBattleTarget = null;
            isDead = true;
            currentHealth = 0;
            OnDied?.Invoke();
        }
    }

    public void RestoreHealth(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}
