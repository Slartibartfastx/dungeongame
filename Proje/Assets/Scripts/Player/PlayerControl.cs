using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Transform weaponShootPosition;
    [SerializeField] private MovementDetails movDetails;

    private Player player;
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

        wepDir = (mouseWorldPos - weaponShootPosition.position);

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
