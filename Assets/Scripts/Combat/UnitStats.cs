using System;
using System.Collections;
using System.Collections.Generic;
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

    public UsePurpose usePurpose;

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

    public void TakeDamage(int damage, UsePurpose usePurpose = UsePurpose.None)
    {
        if (isDead) return;

        if (this.usePurpose == usePurpose)
            damage *= 2;
        
        currentHealth -= damage;

        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
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
