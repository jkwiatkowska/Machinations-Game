using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    float Speed;
    float ArchAngle;
    float Weight;

    const float kGravity = -9.8f;
    
    Vector2 TargetPosition;
    float HitRadius;

    Action<Movement> OnPositionReached;
    Action<Movement> OnHit;

    Vector2 Velocity = Vector2.zero;

    public enum eMovementMode
    {
        Idle,
        Arch,
        Straight
    }

    eMovementMode MovementMode = eMovementMode.Idle;

    public void SetArchMovement(Vector2 targetPosition, float hitRadius, Action<Movement> onPositionReached, Action<Movement> onHit, float archAngle = 40.0f, float weight = 0.4f)
    {
        ArchAngle = archAngle;
        Weight = weight;

        if (targetPosition == null)
        {
            return;
        }

        TargetPosition = targetPosition;
        HitRadius = hitRadius;
        OnPositionReached = onPositionReached;
        OnHit = onHit;

        if (!StartArchMovement())
        {
            return;
        }

        MovementMode = eMovementMode.Arch;
    }

    public void SetStraightMovement(Vector2 targetPosition, float hitRadius, Action<Movement> onPositionReached, Action<Movement> onHit, float speed = 5.0f)
    {
        Speed = speed;

        if (targetPosition == null)
        {
            return;
        }

        TargetPosition = targetPosition;
        HitRadius = hitRadius;
        OnPositionReached = onPositionReached;
        OnHit = onHit;

        var velocity = new Vector2(transform.position.x, transform.position.y);
        velocity = TargetPosition - velocity;
        velocity.Normalize();

        Velocity = velocity * Speed;

        MovementMode = eMovementMode.Straight;
    }

    public void StopMovement()
    {
        MovementMode = eMovementMode.Idle;
    }

    bool StartArchMovement()
    {
        float xDistance = transform.position.x - TargetPosition.x;
        if (Mathf.Abs(xDistance) < Utility.Epsilon)
        {
            Debug.Log("x distance is too small to launch.");
            return false;
        }

        float yDistance = Mathf.Abs(TargetPosition.y - transform.position.y);
        float tanAlpha = Mathf.Tan(ArchAngle * Mathf.Deg2Rad);

        float xSpeed = Mathf.Sqrt(kGravity * Weight * xDistance * xDistance / (2.0f * (yDistance - xDistance * tanAlpha)));
        if (float.IsNaN(xSpeed))
        {
            Debug.Log("xSpeed is not a number.");
            return false;
        }
        float ySpeed = tanAlpha * xSpeed;

        Velocity = new Vector2(-xSpeed, ySpeed);

        MovementMode = eMovementMode.Arch;

        return true;
    }

    void Update()
    {
        if (MovementMode == eMovementMode.Idle)
        {
            return;
        }

        if (MovementMode == eMovementMode.Arch)
        {
            Velocity.y += kGravity * Weight * Time.deltaTime;
        }

        var newPos = transform.position;
        newPos.x += Velocity.x * Time.deltaTime;
        newPos.y += Velocity.y * Time.deltaTime;

        var targetReached = (Utility.IsBetween(transform.position, TargetPosition, newPos, HitRadius));

        transform.position = newPos;

        if (targetReached)
        {
            OnPositionReached?.Invoke(this);
            OnPositionReached = null;
        }
    }

    public void InvokeOnHit()
    {
        OnHit?.Invoke(this);
    }
}
