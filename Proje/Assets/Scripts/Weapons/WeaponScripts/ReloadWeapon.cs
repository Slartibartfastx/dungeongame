using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]

[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    private ReloadWeaponEvent reloadEvent;
    private WeaponReloadedEvent reloadedEvent;
    private SetActiveWeaponEvent activeWeaponEvent;
    private Coroutine reloadCoroutine;

    private void Awake()
    {
        // Load components
        reloadEvent = GetComponent<ReloadWeaponEvent>();
        reloadedEvent = GetComponent<WeaponReloadedEvent>();
        activeWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        // subscribe to reload weapon event
        reloadEvent.OnReloadWeapon += OnReloadWeaponEvent;

        // Subscribe to set active weapon event
        activeWeaponEvent.OnSetActiveWeapon += OnSetActiveWeaponEvent;
    }

    private void OnDisable()
    {
        // unsubscribe from reload weapon event
        reloadEvent.OnReloadWeapon -= OnReloadWeaponEvent;

        // Unsubscribe from set active weapon event
        activeWeaponEvent.OnSetActiveWeapon -= OnSetActiveWeaponEvent;
    }

    /// <summary>
    /// Handle reload weapon event
    /// </summary>
    private void OnReloadWeaponEvent(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs eventArgs)
    {
        StartWeaponReload(eventArgs);
    }

    /// <summary>
    /// Start reloading the weapon
    /// </summary>
    private void StartWeaponReload(ReloadWeaponEventArgs eventArgs)
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }

        reloadCoroutine = StartCoroutine(ReloadWeaponRoutine(eventArgs.weapon, eventArgs.topUpAmmoPercent));
    }

    /// <summary>
    /// Reload weapon coroutine
    /// </summary>
    private IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpPercentage)
    {
       
        // Set weapon as reloading
        weapon.isReloading = true;

        // Update reload progress timer
        while (weapon.wepReloadTimer < weapon.wepDetails.reloadDuration)
        {
            weapon.wepReloadTimer += Time.deltaTime;
            yield return null;
        }

        // If total ammo is to be increased then update
        if (topUpPercentage != 0)
        {
            int ammoToAdd = Mathf.RoundToInt((weapon.wepDetails.maxAmmo * topUpPercentage) / 100f);

            int updatedTotalAmmo = weapon.wepRemainingAmmo + ammoToAdd;

            if (updatedTotalAmmo > weapon.wepDetails.maxAmmo)
            {
                weapon.wepRemainingAmmo = weapon.wepDetails.maxAmmo;
            }
            else
            {
                weapon.wepRemainingAmmo = updatedTotalAmmo;
            }
        }

        // If weapon has infinite ammo then just refill the clip
        if (weapon.wepDetails.infiniteAmmo)
        {
            weapon.wepClipRemainingAmmo = weapon.wepDetails.clipCapacity;
        }
        // else if not infinite ammo then if remaining ammo is greater than the amount required to
        // refill the clip, then fully refill the clip
        else if (weapon.wepRemainingAmmo >= weapon.wepDetails.clipCapacity)
        {
            weapon.wepClipRemainingAmmo = weapon.wepDetails.clipCapacity;
        }
        // else set the clip to the remaining ammo
        else
        {
            weapon.wepClipRemainingAmmo = weapon.wepRemainingAmmo;
        }

        // Reset weapon reload timer
        weapon.wepReloadTimer = 0f;

        // Set weapon as not reloading
        weapon.isReloading = false;

        // Call weapon reloaded event
        reloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Set active weapon event handler
    /// </summary>
    private void OnSetActiveWeaponEvent(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs eventArgs)
    {
        if (eventArgs.wep.isReloading)
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
            }

            reloadCoroutine = StartCoroutine(ReloadWeaponRoutine(eventArgs.wep, 0));
        }
    }
}
