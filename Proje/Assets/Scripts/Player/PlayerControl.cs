using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Player))]
public class PlayerControl : MonoBehaviour
{
    
    [SerializeField] private MovementDetails movDetails;

    private Player player;
    private int currWepInd = 1;
    private float movSpeed;

    private void Awake()
    {
        player = GetComponent<Player>();

        movSpeed = movDetails.GetMoveSpeed();
    }

    private void Update()
    {
        MovInput();
        WepInput();
    }

    private void Start()
    {

        // Set Starting Weapon
        SetStartingWeapon();

        // Set player animation speed
        SetPlayerAnimationSpeed();

    }

    private void SetStartingWeapon()
    {
        int index = 1;

        foreach (Weapon weapon in player.wepList)
        {
            if (weapon.wepDetails == player.playerDetails.startingWeapon)
            {
                SetWepByIndex(index);
                break;
            }
            index++;
        }
    }

    private void SetWepByIndex(int index)
    {
        if (index - 1 < player.wepList.Count)
        {
            currWepInd = index;
            player.setActiveWepEvent.CallSetActiveWeaponEvent(player.wepList[index - 1]);
        }
    }

    /// <summary>
    /// Weapon Input
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDir playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Reload weapon input
        //ReloadWeaponInput();
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDir playerAimDirection)
    {
        // Get mouse world position
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // Calculate direction vector of mouse cursor from weapon shoot position
        weaponDirection = (mouseWorldPosition - player.activeWep.GetShootPosition());

        // Calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // Get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.getAngleFromVec(weaponDirection);

        // Get player to cursor angle
        playerAngleDegrees = HelperUtilities.getAngleFromVec(playerDirection);

        // Set player aim direction
        playerAimDirection = HelperUtilities.getAimdir(playerAngleDegrees);

        // Trigger weapon aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDir playerAimDirection)
    {
        
        // Fire when left mouse button is clicked
        if (Input.GetMouseButton(0))
        {
            Debug.Log("fired");
            // Trigger fire weapon event
            player.fireWepEvent.CallFireWeaponEvent(true, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
           
        }
    }

    /// <summary>
    /// Set player animator speed to match movement speed
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        // Set animator speed to match movement speed
        player.animator.speed = movSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void MovInput()
    {
        float horizontalMov = Input.GetAxis("Horizontal");
        float verticalMov = Input.GetAxis("Vertical");

        Vector2 dir = new Vector2(horizontalMov, verticalMov);

        if (horizontalMov != 0f && verticalMov != 0f)
        {
            dir *= 0.7f;
        }

        if (dir != Vector2.zero)
        {
            player.movByVelEvent.CallMovementByVelocityEvent(dir, movSpeed);

        }
        else
        {
            player.idleEvent.CallIdleEvent();
        }
        
    }

    private void WepInput()
    {
        Vector3 wepDir;
        float wepAngDeg, playerAngDeg;
        AimDir playerAimDir;

        AimWepInput(out wepDir, out wepAngDeg, out playerAngDeg, out playerAimDir);


    }

    private void AimWepInput(out Vector3 wepDir, out float wepAngDeg, out float playerAngDeg, out AimDir playerAimDir)
    {
        Vector3 mouseWorldPos = HelperUtilities.getMousePos();
        
        wepDir = (mouseWorldPos - player.activeWep.GetShootPosition());

        Vector3 playerDir = (mouseWorldPos - transform.position);

        wepAngDeg = HelperUtilities.getAngleFromVec(wepDir);
        playerAngDeg = HelperUtilities.getAngleFromVec(playerDir);

        playerAimDir = HelperUtilities.getAimdir(playerAngDeg);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDir, playerAngDeg, wepAngDeg, wepDir);
    }

    #region VALIDATION

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movDetails), movDetails);
    }
#endif
    #endregion
}
