using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D rBody2d;
    private MovementByVelocityEvent movVelEvent;

    private void Awake()
    {
        // Load components
        rBody2d = GetComponent<Rigidbody2D>();
        movVelEvent = GetComponent<MovementByVelocityEvent>();
    }

    private void OnEnable()
    {
        movVelEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void OnDisable()
    {
     
        movVelEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;
    }

    // hareket eventinde
    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        MoveRigidBody(movementByVelocityArgs.moveDirection, movementByVelocityArgs.moveSpeed);
    }


    /// rigidbodyi hareket ettir
    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {

        rBody2d.velocity = moveDirection * moveSpeed;
    }
}
