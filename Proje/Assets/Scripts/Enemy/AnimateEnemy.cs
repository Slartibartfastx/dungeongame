using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class AnimateEnemy : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        // Load components
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        // Subscribe to movement event
        enemy.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;

        // Subscribe to idle event
        enemy.idleEvent.OnIdle += IdleEvent_OnIdle;

        // Subscribe to weapon aim event
        enemy.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        // Unsubscribe from movement event
        enemy.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;

        // Unsubscribe from idle event
        enemy.idleEvent.OnIdle -= IdleEvent_OnIdle;

        // Unsubscribe from weapon aim event event
       enemy.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }


   private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitialiseAimAnimationParameters();
        SetAimWeaponAnimationParameters(aimWeaponEventArgs.aimDirection);
    }

 
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
       /* if (enemy.transform.position.x < GameManager.Instance.GetPlayer().transform.position.x)
        {
            SetAimWeaponAnimationParameters(AimDir.Right);
        }
        else
        {
            SetAimWeaponAnimationParameters(AimDir.Left);
        }*/
        SetMovementAnimationParameters();
    }


    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

  
    private void InitialiseAimAnimationParameters()
    {
        enemy.animator.SetBool(Settings.aimUp, false);
        enemy.animator.SetBool(Settings.aimUpRight, false);
        enemy.animator.SetBool(Settings.aimUpLeft, false);
        enemy.animator.SetBool(Settings.aimRight, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
        enemy.animator.SetBool(Settings.aimDown, false);
    }

 
    private void SetMovementAnimationParameters()
    {
        // Set Moving
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, true);
    }



    private void SetIdleAnimationParameters()
    {
        // Set idle
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
    }

 
    private void SetAimWeaponAnimationParameters(AimDir aimDirection)
    {
        // Set aim direction
        switch (aimDirection)
        {
            case AimDir.Up:
                enemy.animator.SetBool(Settings.aimUp, true);
                break;

            case AimDir.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                break;

            case AimDir.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                break;

            case AimDir.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                enemy.animator.SetBool(Settings.aimLeft, false);
                break;

            case AimDir.Left:
                enemy.animator.SetBool(Settings.aimRight, false);
                enemy.animator.SetBool(Settings.aimLeft, true);
                break;

            case AimDir.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                break;
        }
    }
}