using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimPlayer : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        // Load components
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.movByVelEvent.OnMovementByVelocity += MovByVelEvent_OnMovByVel;

        // Subscribe to idle event
        player.idleEvent.OnIdle += IdleEvent_OnIdle;

        // Subscribe to weapon aim event
        player.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;

  
    }

    private void OnDisable()
    {
        player.movByVelEvent.OnMovementByVelocity -= MovByVelEvent_OnMovByVel;

        // Unsubscribe from idle event
        player.idleEvent.OnIdle -= IdleEvent_OnIdle;

        // Unsubscribe from weapon aim event event
        player.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

  

 
    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {

        SetIdleAnimationParameters();
    }

    private void MovByVelEvent_OnMovByVel(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        SetMovementAnimationParameters();
    }

    
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitializeAimAnimationParameters();

        SetAimWeaponAnimationParameters(aimWeaponEventArgs.aimDirection);
    }


    private void InitializeAimAnimationParameters()
    {
        player.animator.SetBool(Settings.aimUp, false);
        player.animator.SetBool(Settings.aimUpRight, false);
        player.animator.SetBool(Settings.aimUpLeft, false);
        player.animator.SetBool(Settings.aimRight, false);
        player.animator.SetBool(Settings.aimLeft, false);
        player.animator.SetBool(Settings.aimDown, false);
    }



   
    private void SetMovementAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);
    }

 
    

  
    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
    }

    private void SetAimWeaponAnimationParameters(AimDir aimDirection)
    {
        // Set aim direction
        switch (aimDirection)
        {
            case AimDir.Up:
                player.animator.SetBool(Settings.aimUp, true);
                break;

            case AimDir.UpRight:
                player.animator.SetBool(Settings.aimUpRight, true);
                break;

            case AimDir.UpLeft:
                player.animator.SetBool(Settings.aimUpLeft, true);
                break;

            case AimDir.Right:
                player.animator.SetBool(Settings.aimRight, true);
                break;

            case AimDir.Left:
                player.animator.SetBool(Settings.aimLeft, true);
                break;

            case AimDir.Down:
                player.animator.SetBool(Settings.aimDown, true);
                break;

        }

    }

}
