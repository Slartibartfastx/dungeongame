using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    [SerializeField] private Transform weaponRotationPointTransform;

    private AimWeaponEvent aimWeaponEvent;

    private void Awake()
    {
        // componentleri yukle
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        // aimweaponevent'e subscribe ol
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        // unsubscribe
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }


    /// event handler
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }


    /// nisan alma

    private void Aim(AimDir aimDirection, float aimAngle)
    {
        // aciyi ayarla
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        // oyuncunun yonune gore nisan yonunu belirle
        switch (aimDirection)
        {
            case AimDir.Left:
            case AimDir.UpLeft:
                weaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;

            case AimDir.Up:
            case AimDir.UpRight:
            case AimDir.Right:
            case AimDir.Down:
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }

    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }
#endif
    #endregion

}
