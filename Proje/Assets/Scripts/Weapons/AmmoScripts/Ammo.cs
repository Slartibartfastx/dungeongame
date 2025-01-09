using UnityEngine;

[DisallowMultipleComponent]
public class Ammo : MonoBehaviour, IFireable
{
    [SerializeField] private TrailRenderer trailEffect;

    private float range = 0f; // The range of the ammo
    private float speed;
    private Vector3 fireDirection;
    private float fireAngle;
    private SpriteRenderer spriteRenderer;
    private AmmoDetails details;
    private float chargeTimer;
    private bool isMaterialSet = false;
    private bool overrideMovement;
    private bool isColliding = false;

    private void Awake()
    {
        // Cache sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Handle ammo charge effect
        if (chargeTimer > 0f)
        {
            chargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isMaterialSet)
        {
            SetMaterial(details.projectileMaterial);
            isMaterialSet = true;
        }

        // Skip movement if overridden
        if (!overrideMovement)
        {
            Vector3 moveDistance = fireDirection * speed * Time.deltaTime;

            transform.position += moveDistance;

            range -= moveDistance.magnitude;

            if (range < 0f)
            {
               /* if (details.isPlayerProjectile)
                {
                    StaticEventHandler.CallMultiplierEvent(false);
                }*/

                Disable();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isColliding) return;

        //DealDamage(collision);
        //ShowHitEffect();
        Disable();
    }

   /* private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();
        bool enemyHit = false;

        if (health != null)
        {
            isColliding = true;
            health.TakeDamage(details.damage);

            if (health.enemy != null)
            {
                enemyHit = true;
            }
        }

        if (details.isPlayerProjectile)
        {
            StaticEventHandler.CallMultiplierEvent(enemyHit);
        }
    }*/

    public void InitialiseAmmo(AmmoDetails ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirection, bool overrideMovement = false)
    {
        this.details = ammoDetails;

        isColliding = false;

        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirection);

        spriteRenderer.sprite = ammoDetails.projectileSprite;

        if (ammoDetails.chargeTime > 0f)
        {
            chargeTimer = ammoDetails.chargeTime;
            SetMaterial(ammoDetails.chargeMaterial);
            isMaterialSet = false;
        }
        else
        {
            chargeTimer = 0f;
            SetMaterial(ammoDetails.projectileMaterial);
            isMaterialSet = true;
        }

        range = ammoDetails.range;
        this.speed = ammoSpeed;
        this.overrideMovement = overrideMovement;

        gameObject.SetActive(true);

        if (ammoDetails.hasTrail)
        {
            trailEffect.gameObject.SetActive(true);
            trailEffect.emitting = true;
            trailEffect.material = ammoDetails.trailMaterial;
            trailEffect.startWidth = ammoDetails.trailStartWidth;
            trailEffect.endWidth = ammoDetails.trailEndWidth;
            trailEffect.time = ammoDetails.trailLifetime;
        }
        else
        {
            trailEffect.emitting = false;
            trailEffect.gameObject.SetActive(false);
        }
    }

    private void SetFireDirection(AmmoDetails ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirection)
    {
        float randomSpread = Random.Range(ammoDetails.spreadMin, ammoDetails.spreadMax);
        int spreadToggle = Random.Range(0, 2) * 2 - 1;

        fireAngle = weaponAimDirection.magnitude < Settings.useAimAngleDistance ? aimAngle : weaponAimAngle;

        fireAngle += spreadToggle * randomSpread;

        transform.eulerAngles = new Vector3(0f, 0f, fireAngle);

       fireDirection = HelperUtilities.getDirVecFromAng(fireAngle);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }

   /* private void ShowHitEffect()
    {
        if (details.ammoHitEffect != null && details.ammoHitEffect.hitEffectPrefab != null)
        {
            AmmoHitEffect hitEffect = (AmmoHitEffect)PoolManager.Instance.ReuseComponent(details.ammoHitEffect.hitEffectPrefab, transform.position, Quaternion.identity);

            hitEffect.SetHitEffect(details.ammoHitEffect);

            hitEffect.gameObject.SetActive(true);
        }
    }*/

    public void SetMaterial(Material material)
    {
        spriteRenderer.material = material;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailEffect), trailEffect);
    }
#endif
    #endregion
}
