using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetails : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("WEAPON BASE DETAILS")]
    #endregion Header WEAPON BASE DETAILS

    public string weaponTitle; //Weapon Name
    public Sprite weaponIcon;  //Weapon Sprite

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION

    public Vector3 shootPositionOffset;
    public AmmoDetails currentAmmo;
    /* public WeaponShootEffectSO shootEffect;
     public SoundEffectSO firingSound;
     public SoundEffectSO reloadSound;*/

    #region Header WEAPON OPERATING VALUES
    [Space(10)]
    [Header("WEAPON OPERATING VALUES")]
    #endregion Header WEAPON OPERATING VALUES

    public bool infiniteAmmo = false;
    public bool infiniteClip = false;
    public int clipCapacity = 6;
    public int maxAmmo = 100;
    public float fireRate = 0.2f;
    public float prechargeTime = 0f;
    public float reloadDuration = 0f;

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponTitle), weaponTitle);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentAmmo), currentAmmo);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(fireRate), fireRate, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(prechargeTime), prechargeTime, true);

        if (!infiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(maxAmmo), maxAmmo, false);
        }

        if (!infiniteClip)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(clipCapacity), clipCapacity, false);
        }
    }

#endif
    #endregion Validation
}
