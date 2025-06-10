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

    public event Action OnDamaged;
    public event Action OnDied;

    public void Init()
    {
        if (statusSO != null)
        {
            maxHealth = statusSO.MaxHp;
            maxStamina = statusSO.MaxStamina;

            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDied?.Invoke();
        }
    }

    public void RestoreHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}
