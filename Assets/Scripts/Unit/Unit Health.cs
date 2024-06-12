using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{

    public event EventHandler OnDead;
    public event EventHandler OnDamaged;

    [SerializeField] private int health;
    [SerializeField] private int maxHealth;

    void Awake()
    {
        health = maxHealth;
    }

    public void Damage(int amount)
    {
        health -= amount;

        if(health < 0)
        {
            health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);

        if(health == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float) health / maxHealth;
    }

    public int GetRemainingHealth()
    {
        return health;
    }

    //TODO
    public bool IsLethalHit(int damageAmount)
    {
        return damageAmount >= health;
    }
}
