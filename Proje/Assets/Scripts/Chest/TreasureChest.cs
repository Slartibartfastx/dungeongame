using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MaterializeEffect))]
public class TreasureChest : MonoBehaviour, IUseable
{
    [Tooltip("Set this to the color to be used for the materialization effect")]
    [ColorUsage(false, true)]
    [SerializeField] private Color materializationColor;

    [Tooltip("Set this to the time it will take to materialize the chest")]
    [SerializeField] private float materializationDuration = 3f;

    [Tooltip("Populate with item spawn point transform")]
    [SerializeField] private Transform spawnLocation;

    private int healthBonus;
    public WeaponDetails equippedWeaponDetails;
    private int ammoBonus;
    private Animator chestAnimator;
    private SpriteRenderer chestSpriteRenderer;
    private MaterializeEffect materializationEffect;
    private bool isActive = false;
    private ChestState currentChestState = ChestState.closed;
    private GameObject chestItemObject;
    private LootItem chestItemInstance;
    private TextMeshPro chestMessageText;

    private void Awake()
    {
        chestAnimator = GetComponent<Animator>();
        chestSpriteRenderer = GetComponent<SpriteRenderer>();
        materializationEffect = GetComponent<MaterializeEffect>();
        chestMessageText = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        InitializeChest(true, 50, equippedWeaponDetails, 60);
    }

    public void InitializeChest(bool shouldMaterialize, int healthBonus, WeaponDetails equippedWeaponDetails, int ammoBonus)
    {
        this.healthBonus = healthBonus;
        this.equippedWeaponDetails = equippedWeaponDetails;
        this.ammoBonus = ammoBonus;

        if (shouldMaterialize)
        {
            StartCoroutine(Materialize());
        }
        else
        {
            ActivateChest();
        }
    }

    private IEnumerator Materialize()
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { chestSpriteRenderer };

        yield return StartCoroutine(materializationEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializationColor, materializationDuration, spriteRendererArray, GameResources.Instance.litMaterial));

        ActivateChest();
    }

    private void ActivateChest()
    {
        isActive = true;
    }

    public void UseItem()
    {
        if (!isActive) return;

        switch (currentChestState)
        {
            case ChestState.closed:
                Open();
                break;

            case ChestState.healthItem:
                CollectHealth();
                break;

            case ChestState.ammoItem:
                CollectAmmo();
                break;

            case ChestState.weaponItem:
                CollectWeapon();
                break;

            case ChestState.empty:
                return;

            default:
                return;
        }
    }

    private void Open()
    {
        chestAnimator.SetBool(Settings.use, true);

        if (equippedWeaponDetails != null)
        {
            if (GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(equippedWeaponDetails))
                equippedWeaponDetails = null;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        if (healthBonus != 0)
        {
            currentChestState = ChestState.healthItem;
            SpawnHealth();
        }
        else if (ammoBonus != 0)
        {
            currentChestState = ChestState.ammoItem;
            SpawnAmmo();
        }
        else if (equippedWeaponDetails != null)
        {
            currentChestState = ChestState.weaponItem;
            SpawnWeapon();
        }
        else
        {
            currentChestState = ChestState.empty;
        }
    }

    private void SpawnItem()
    {
        chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, this.transform);
        chestItemInstance = chestItemObject.GetComponent<LootItem>();
    }

    private void SpawnHealth()
    {
        SpawnItem();
        chestItemInstance.Setup(GameResources.Instance.heartIcon, healthBonus.ToString() + "%", spawnLocation.position, materializationColor);
    }

    private void CollectHealth()
    {
        if (chestItemInstance == null || !chestItemInstance.hasMaterialized) return;

        GameManager.Instance.GetPlayer().hp.AddHealth(healthBonus);

        healthBonus = 0;

        Destroy(chestItemObject);

        UpdateState();
    }

    private void SpawnAmmo()
    {
        SpawnItem();
        chestItemInstance.Setup(GameResources.Instance.bulletIcon, ammoBonus.ToString() + "%", spawnLocation.position, materializationColor);
    }

    private void CollectAmmo()
    {
        if (chestItemInstance == null || !chestItemInstance.hasMaterialized) return;

        Player player = GameManager.Instance.GetPlayer();

        player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWep.GetCurrentWeapon(), ammoBonus);

        ammoBonus = 0;

        Destroy(chestItemObject);

        UpdateState();
    }

    private void SpawnWeapon()
    {
        SpawnItem();
        chestItemObject.GetComponent<LootItem>().Setup(equippedWeaponDetails.weaponIcon, equippedWeaponDetails.weaponTitle, spawnLocation.position, materializationColor);
    }

    private void CollectWeapon()
    {
        if (chestItemInstance == null || !chestItemInstance.hasMaterialized) return;

        if (!GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(equippedWeaponDetails))
        {
            GameManager.Instance.GetPlayer().AddWepToPlayer(equippedWeaponDetails);
        }
        else
        {
            StartCoroutine(DisplayChestMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        equippedWeaponDetails = null;

        Destroy(chestItemObject);

        UpdateState();
    }

    private IEnumerator DisplayChestMessage(string message, float duration)
    {
        chestMessageText.text = message;

        yield return new WaitForSeconds(duration);

        chestMessageText.text = "";
    }
}
