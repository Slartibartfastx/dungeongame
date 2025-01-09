using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")]
public class AmmoDetails : ScriptableObject
{
    #region Header BASIC AMMO DETAILS
    [Space(10)]
    [Header("BASIC AMMO DETAILS")]
    #endregion

    public string ammoTitle;
    public bool isPlayerProjectile;

    #region Header AMMO SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("AMMO SPRITE, PREFAB & MATERIALS")]
    #endregion

    public Sprite projectileSprite;
    public GameObject[] projectilePrefabs;
    public Material projectileMaterial;
    public float chargeTime = 0.1f;
    public Material chargeMaterial;

    #region Header AMMO HIT EFFECT
    [Space(10)]
    [Header("AMMO HIT EFFECT")]
    #endregion

   // public AmmoHitEffect hitEffect;

    #region Header AMMO BASE PARAMETERS
    [Space(10)]
    [Header("AMMO BASE PARAMETERS")]
    #endregion

    public int damage = 1;
    public float speedMin = 20f;
    public float speedMax = 20f;
    public float range = 20f;
    public float rotationSpeed = 1f;

    #region Header AMMO SPREAD DETAILS
    [Space(10)]
    [Header("AMMO SPREAD DETAILS")]
    #endregion

    public float spreadMin = 0f;
    public float spreadMax = 0f;

    #region Header AMMO SPAWN DETAILS
    [Space(10)]
    [Header("AMMO SPAWN DETAILS")]
    #endregion

    public int spawnAmountMin = 1;
    public int spawnAmountMax = 1;
    public float spawnIntervalMin = 0f;
    public float spawnIntervalMax = 0f;

    #region Header AMMO TRAIL DETAILS
    [Space(10)]
    [Header("AMMO TRAIL DETAILS")]
    #endregion

    public bool hasTrail = false;
    public float trailLifetime = 3f;
    public Material trailMaterial;
    [Range(0f, 1f)] public float trailStartWidth;
    [Range(0f, 1f)] public float trailEndWidth;

    #region Validation
#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoTitle), ammoTitle);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileSprite), projectileSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(projectilePrefabs), projectilePrefabs);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileMaterial), projectileMaterial);
        if (chargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(chargeMaterial), chargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(damage), damage, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(speedMin), speedMin, nameof(speedMax), speedMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(range), range, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(spreadMin), spreadMin, nameof(spreadMax), spreadMax, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(spawnAmountMin), spawnAmountMin, nameof(spawnAmountMax), spawnAmountMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(spawnIntervalMin), spawnIntervalMin, nameof(spawnIntervalMax), spawnIntervalMax, true);
        if (hasTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(trailLifetime), trailLifetime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(trailMaterial), trailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(trailStartWidth), trailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(trailEndWidth), trailEndWidth, false);
        }
    }
#endif
    #endregion
}
