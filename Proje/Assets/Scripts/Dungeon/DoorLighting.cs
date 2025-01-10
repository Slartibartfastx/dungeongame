using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorLighting : MonoBehaviour
{
    private bool isLit = false;
    private DoorScript door;
    private Material fadeMaterial;

    private void Awake()
    {
        // Get components
        door = GetComponentInParent<DoorScript>();

        // Cache the fade material
        fadeMaterial = new Material(GameResources.Instance.variableLitShader);
    }

 
    public void FadeInDoor()
    {
        if (isLit) return;

        SpriteRenderer[] spriteRenderers = GetComponentsInParent<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            StartCoroutine(FadeInDoorRoutine(spriteRenderer));
        }

        isLit = true;
    }


    private IEnumerator FadeInDoorRoutine(SpriteRenderer spriteRenderer)
    {
        // Assign fade material
        spriteRenderer.material = fadeMaterial;

        // Perform fade effect
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime / Settings.fadeInTime;
            fadeMaterial.SetFloat("Alpha_Slider", Mathf.Clamp01(alpha));
            yield return null;
        }

        // Restore lit material after fading
        spriteRenderer.material = GameResources.Instance.litMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon))
        {
            FadeInDoor();
        }
    }
}
