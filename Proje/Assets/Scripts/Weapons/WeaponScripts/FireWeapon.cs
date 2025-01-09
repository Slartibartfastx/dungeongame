using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
//[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    private float firePreChargeTimer = 0f;
    private float fireRateCoolDownTimer = 0f;
    private ActiveWeapon activeWeapon;
    private FireWeaponEvent fireWeaponEvent;
    //private ReloadWeaponEvent reloadWeaponEvent;
    private WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        // Load components.
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        //reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
    }

    private void OnEnable()
    {
        // Subscribe to fire weapon event.
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        // Unsubscribe from fire weapon event.
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        // Decrease cooldown timer.
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Handle fire weapon event.
    /// </summary>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    /// <summary>
    /// Fire weapon.
    /// </summary>
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Handle weapon precharge timer.
        WeaponPreCharge(fireWeaponEventArgs);

        // Weapon fire.
        if (fireWeaponEventArgs.fire)
        {
            // Test if weapon is ready to fire.
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);

                ResetCoolDownTimer();

                
            }
        }
    }

    /// <summary>
    /// Handle weapon precharge.
    /// </summary>
    private void WeaponPreCharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Weapon precharge.
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            // Decrease precharge timer if fire button held previous frame.
            firePreChargeTimer -= Time.deltaTime;
        }
        else
        {
       
        }
    }

    /// <summary>
    /// Returns true if the weapon is ready to fire, else returns false.
    /// </summary>
    private bool IsWeaponReadyToFire()
    {
        // if there is no ammo and weapon doesn't have infinite ammo then return false.
        if (activeWeapon.GetCurrentWeapon().wepRemainingAmmo <= 0 && !activeWeapon.GetCurrentWeapon().wepDetails.infiniteAmmo)
            return false;

        // if the weapon is reloading then return false.
        if (activeWeapon.GetCurrentWeapon().isReloading)
            return false;

        // If the weapon isn't precharged or is cooling down then return false.
        if (firePreChargeTimer > 0f || fireRateCoolDownTimer > 0f)
            return false;

        // if no ammo in the clip and the weapon doesn't have infinite clip capacity then return false.
        if (!activeWeapon.GetCurrentWeapon().wepDetails.infiniteClip && activeWeapon.GetCurrentWeapon().wepClipRemainingAmmo <= 0)
        {
            // trigger a reload weapon event.
            //reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentWeapon(), 0);

            return false;
        }

        // weapon is ready to fire - return true
        return true;
    }

    /// <summary>
    /// Set up ammo using an ammo gameobject and component from the object pool.
    /// </summary>
    private void FireAmmo(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        AmmoDetails currentAmmo = activeWeapon.GetCurrentAmmo();

        if (currentAmmo != null)
        {
            // Get ammo prefab from array
            GameObject ammoPrefab = currentAmmo.projectilePrefabs[Random.Range(0, currentAmmo.projectilePrefabs.Length)];

            // Get random speed value
            float ammoSpeed = Random.Range(currentAmmo.speedMin, currentAmmo.speedMax);

            // Get GameObject with IFireable component
            IFireable ammo = (IFireable)PoolManager.Instance.Reuse(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);

            // Initialise Ammo
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);
            Debug.Log("fired");
            // Reduce ammo clip count if not infinite clip capacity
            if (!activeWeapon.GetCurrentWeapon().wepDetails.infiniteClip)
            {
                activeWeapon.GetCurrentWeapon().wepClipRemainingAmmo--;
                activeWeapon.GetCurrentWeapon().wepRemainingAmmo--;
            }

            // Call weapon fired event
            weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentWeapon());
        }
    }


    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCoolDownTimer()
    {
        // Reset cooldown timer
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().wepDetails.fireRate;
    }

}