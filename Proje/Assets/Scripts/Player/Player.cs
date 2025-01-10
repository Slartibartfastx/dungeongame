using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(AnimPlayer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
#endregion REQUIRE COMPONENTS
public class Player : MonoBehaviour
{

    [HideInInspector] public PlayerDetails playerDetails;
    [HideInInspector] public MovementByVelocityEvent movByVelEvent;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Health hp;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public SetActiveWeaponEvent setActiveWepEvent;
    [HideInInspector] public ActiveWeapon activeWep;
    [HideInInspector] public FireWeaponEvent fireWepEvent;
    [HideInInspector] public WeaponFiredEvent wepFiredEvent;
    [HideInInspector] public FireWeapon fireWep;
    [HideInInspector] public ReloadWeaponEvent reloadWeaponEvent;
    [HideInInspector] public WeaponReloadedEvent weaponReloadedEvent;
    [HideInInspector] public ReloadWeapon reloadWeapon;
    public List<Weapon> wepList = new List<Weapon>();
    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        movByVelEvent = GetComponent<MovementByVelocityEvent>();
        hp = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        idleEvent = GetComponent<IdleEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        setActiveWepEvent = GetComponent<SetActiveWeaponEvent>();
        activeWep = GetComponent<ActiveWeapon>();
        fireWepEvent = GetComponent<FireWeaponEvent>();
        wepFiredEvent = GetComponent<WeaponFiredEvent>(); 
        fireWep = GetComponent<FireWeapon>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent= GetComponent<WeaponReloadedEvent>();
        reloadWeapon = GetComponent<ReloadWeapon>();
    }

    public void Initalize(PlayerDetails playerDetailss)
    {
        this.playerDetails = playerDetailss;
        CreatePlayerStartingWeps();
        setPlayerHP();
    }

    private void OnEnable()
    {
        // Subscribe to player health event
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from player health event
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }
    /// <summary>
    /// Handle health changed event
    /// </summary>
    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // If player has died
        if (healthEventArgs.healthAmount <= 0f)
        {
            destroyedEvent.CallDestroyedEvent(true/*, 0*/);
        }

    }

    public void setPlayerHP()
    {
        hp.setStartingHP(playerDetails.playerHealthAmount);
    }

    /// <summary>
    /// Returns the player position
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    private void CreatePlayerStartingWeps()
    {
        wepList.Clear();

        foreach (WeaponDetails wepDetails in playerDetails.startingWeaponList)
        {
                AddWepToPlayer(wepDetails);
        }
    }


    public Weapon AddWepToPlayer(WeaponDetails wepDetails)
    {
        Weapon weapon = new Weapon() { wepDetails = wepDetails, wepReloadTimer = 0f, wepClipRemainingAmmo = wepDetails.clipCapacity, wepRemainingAmmo = wepDetails.maxAmmo, isReloading = false };

        // Add the weapon to the list
        wepList.Add(weapon);
            
        // Set weapon position in list
        weapon.wepListPosition = wepList.Count;

        // Set the added weapon as active
        setActiveWepEvent.CallSetActiveWeaponEvent(weapon);

        return weapon;
    }
}
