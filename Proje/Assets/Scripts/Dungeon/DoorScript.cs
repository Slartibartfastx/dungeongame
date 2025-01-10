using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class DoorScript : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Header("OBJECT REFERENCES")]
    #endregion
    [Tooltip("Populate this with the BoxCollider2D component on the DoorCollider gameobject")]
    [SerializeField] private BoxCollider2D doorCollider;

    private BoxCollider2D doorTrigger;
    private Animator animator;

    private bool isOpen = false;
    private bool previouslyOpened = false;

    [HideInInspector] public bool isBossRoomDoor = false;

    private void Awake()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();

        // Set default states
        UpdateDoorState(false);
    }

    private void OnEnable()
    {
        // Restore animator state on re-enabling the object
        animator.SetBool(Settings.open, isOpen);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Open the door if the player or their weapon interacts
        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon))
        {
            OpenDoor();
        }
    }

 
    public void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        previouslyOpened = true;

        UpdateDoorState(false);

        // Play door open animation
        animator.SetBool(Settings.open, true);

        // Play sound effect (uncomment if applicable)
        // SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.doorOpenCloseSoundEffect);
    }


    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;

        // set open to false to close door
        animator.SetBool(Settings.open, false);
    }
 
    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;

        if (previouslyOpened == true)
        {
            isOpen = false;
            OpenDoor();
        }
    }


    /// <param name="isLocked">Indicates if the door should be locked</param>
    private void UpdateDoorState(bool isLocked)
    {
        doorCollider.enabled = isLocked;
        doorTrigger.enabled = !isLocked;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}
