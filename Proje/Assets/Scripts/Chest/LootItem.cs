using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MaterializeEffect))]
public class LootItem : MonoBehaviour
{
    private SpriteRenderer itemSpriteRenderer;
    private TextMeshPro itemLabelText;
    private MaterializeEffect materializationEffect;

    [HideInInspector] public bool hasMaterialized = false;

    private void Awake()
    {
        // Cache required components
        itemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        itemLabelText = GetComponentInChildren<TextMeshPro>();
        materializationEffect = GetComponent<MaterializeEffect>();
    }

    /// <summary>
    /// Sets up the loot item's visuals and starts the materialization process.
    /// </summary>
    public void Setup(Sprite itemSprite, string displayText, Vector3 spawnLocation, Color materializationColor)
    {
        itemSpriteRenderer.sprite = itemSprite;
        transform.position = spawnLocation;

        StartCoroutine(MaterializeLoot(materializationColor, displayText));
    }

    /// <summary>
    /// Handles the materialization effect of the loot item.
    /// </summary>
    private IEnumerator MaterializeLoot(Color materializationColor, string displayText)
    {
        SpriteRenderer[] renderers = { itemSpriteRenderer };

        yield return StartCoroutine(materializationEffect.MaterializeRoutine(
            GameResources.Instance.materializeShader,
            materializationColor,
            1f,
            renderers,
            GameResources.Instance.litMaterial
        ));

        hasMaterialized = true;

        itemLabelText.text = displayText;
    }
}
