using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Transform weaponShootPosition;


    private Player player;


    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        MovInput();
        WepInput();
    }


    private void MovInput()
    {
        player.idleEvent.CallIdleEvent();
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
}
