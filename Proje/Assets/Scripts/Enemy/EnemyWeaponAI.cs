using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyWeaponAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Select the layers that the enemy bullets will hit")]
    #endregion Tooltip
    [SerializeField] private LayerMask bulletCollisionLayers;

    #region Tooltip
    [Tooltip("Populate this with the WeaponShootPosition child gameobject transform")]
    #endregion Tooltip
    [SerializeField] private Transform shootPosition;

    private Enemy enemyComponent;
    private EnemyDetails enemyDetailsConfig;
    private float shootIntervalTimer;
    private float shootDurationTimer;

    private void Awake()
    {
        // Load Components
        enemyComponent = GetComponent<Enemy>();
    }

    private void Start()
    {
        enemyDetailsConfig = enemyComponent.enemyDetails;

        shootIntervalTimer = CalculateShootInterval();
        shootDurationTimer = CalculateShootDuration();
    }

    private void Update()
    {
        // Update timers
        shootIntervalTimer -= Time.deltaTime;

        // Interval Timer
        if (shootIntervalTimer < 0f)
        {
            if (shootDurationTimer >= 0)
            {
                shootDurationTimer -= Time.deltaTime;

                ShootWeapon();
            }
            else
            {
                // Reset timers
                shootIntervalTimer = CalculateShootInterval();
                shootDurationTimer = CalculateShootDuration();
            }
        }
    }

    /// <summary>
    /// Calculate a random weapon shoot duration between the min and max values
    /// </summary>
    private float CalculateShootDuration()
    {
        // Calculate a random weapon shoot duration
        return Random.Range(enemyDetailsConfig.firingDurationMin, enemyDetailsConfig.firingDurationMax);
    }

    /// <summary>
    /// Calculate a random weapon shoot interval between the min and max values
    /// </summary>
    private float CalculateShootInterval()
    {
        // Calculate a random weapon shoot interval
        return Random.Range(enemyDetailsConfig.firingIntervalMin, enemyDetailsConfig.firingIntervalMax);
    }

    /// <summary>
    /// Fire the weapon
    /// </summary>
    private void ShootWeapon()
    {
        // Player distance
        Vector3 directionToPlayer = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;

        // Calculate direction vector of player from weapon shoot position
        Vector3 shootDirection = (GameManager.Instance.GetPlayer().GetPlayerPosition() - shootPosition.position);

        // Get weapon to player angle
        float weaponAngle = HelperUtilities.getAngleFromVec(shootDirection);

        // Get enemy to player angle
        float enemyAngle = HelperUtilities.getAngleFromVec(directionToPlayer);

        // Set enemy aim direction
        AimDir aimDirection = HelperUtilities.getAimdir(enemyAngle);

        // Trigger weapon aim event
        enemyComponent.aimWeaponEvent.CallAimWeaponEvent(aimDirection, enemyAngle, weaponAngle, shootDirection);

        // Only fire if enemy has a weapon
        if (enemyDetailsConfig.enemyWeapon != null)
        {
            // Get ammo range
            float ammoRange = enemyDetailsConfig.enemyWeapon.currentAmmo.range;

            // Is the player in range
            if (directionToPlayer.magnitude <= ammoRange)
            {
                // Does this enemy require line of sight to the player before firing?
                if (enemyDetailsConfig.firingLineOfSightRequired && !IsPlayerInLineOfSight(shootDirection, ammoRange)) return;

                // Trigger fire weapon event
                enemyComponent.fireWeaponEvent.CallFireWeaponEvent(true, aimDirection, enemyAngle, weaponAngle, shootDirection);
            }
        }
    }

    private bool IsPlayerInLineOfSight(Vector3 shootDirection, float ammoRange)
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(shootPosition.position, (Vector2)shootDirection, ammoRange, bulletCollisionLayers);

        if (raycastHit && raycastHit.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        return false;
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(shootPosition), shootPosition);
    }

#endif
    #endregion Validation
}
