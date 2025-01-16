using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int startHP;
    private int currentHP;
    private HealthEvent healthEvent;


    [HideInInspector] public bool isDamageable = true;

    private void Awake()
    {
        //Load compnents
        healthEvent = GetComponent<HealthEvent>();
    }

    private void Start()
    {
        // Trigger a health event for UI update
        CallHealthEvent(0);

      /*  // Attempt to load enemy / player components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();


        // Get player / enemy hit immunity details
        if (player != null)
        {
            if (player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderer = player.spriteRenderer;
            }
        }
        else if (enemy != null)
        {
            if (enemy.enemyDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.hitImmunityTime;
                spriteRenderer = enemy.spriteRendererArray[0];
            }
        }

        // Enable the health bar if required
        if (enemy != null && enemy.enemyDetails.isHealthBarDisplayed == true && healthBar != null)
        {
            healthBar.EnableHealthBar();
        }
        else if (healthBar != null)
        {
            healthBar.DisableHealthBar();
        }
      */
    }


    public void TakeDamage(int damageAmount)
    {
        // bool isRolling = false;

        /*if (player != null)
            isRolling = player.playerControl.isPlayerRolling;
        */
        if (isDamageable /*&& !isRolling*/)
        {
            currentHP -= damageAmount;
            CallHealthEvent(damageAmount);

            //PostHitImmunity();

            // Set health bar as the percentage of health remaining
            /*if (healthBar != null)
            {
                healthBar.SetHealthBarValue((float)currentHP / (float)startHP);
            }*/
        }
    }
    private void CallHealthEvent(int damageAmount)
    {
        // Trigger health event
        healthEvent.CallHealthChangedEvent(((float)currentHP / (float)startHP), currentHP, damageAmount);
    }



    public void setStartingHP(int startHP)
    {
        this.startHP = startHP;
        currentHP = startHP;
    }

    public int getStartingHP()
    {
        return startHP;

    }

    /// <summary>
    /// Increase health by specified percent
    /// </summary>
    public void AddHealth(int healthPercent)
    {
        int healthIncrease = Mathf.RoundToInt((startHP * healthPercent) / 100f);

        int totalHealth = currentHP + healthIncrease;

        if (totalHealth > startHP)
        {
            currentHP = startHP;
        }
        else
        {
            currentHP = totalHealth;
        }

        CallHealthEvent(0);
    }

}