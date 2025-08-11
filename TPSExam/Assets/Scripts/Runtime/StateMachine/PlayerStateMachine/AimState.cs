using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Collections;
public class AimState : PlayerState
{
    float _elapsedTime = 0f;
    float _lerpDuration = 0.7f; // Time to decelerate to zero speed
    

    public float aimAngleThreshold = 60f;
    public float rotationSpeed = 5f;
    float leftLimit = -50f;   // customize
    float rightLimit = 75f;   // customize

    float _shootCooldown = 1f; 
    float _lastShootTime = 0f;
    float timeSinceLastShot;
    Vector3 currentFlatForward; 
    bool isTurningRoot = false;


    float AimCenterRadius = 2.7f; 
    Vector2 _smoothedMoveInput;
    float _inputLerpSpeed = 10f; // higher = faster response

    public AimState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(context, stateKey)
    {
    }

    public override void EnterState()
    {
        
        _lastShootTime = _shootCooldown;

        _elapsedTime = 0f;
        Context.Animator.CrossFade("Strafe", Context.NextAnimCrossFadeTime);

        // Save the initial facing direction when entering Aim state
        currentFlatForward = GetFlatForward(Context.RootTransform.forward);

        //AimTargetOffset = new Vector3(2, Context.CinemachineOrbitalFollow.TargetOffset.y, Context.CinemachineOrbitalFollow.TargetOffset.z);
     
    }
    Vector3 GetFlatForward(Vector3 forward)
    {
        forward.y = 0;
        return forward.normalized;
    }
    public override void UpdateState()
    {
        if (_elapsedTime < _lerpDuration)
        {
            _elapsedTime += Time.deltaTime;
            float lerpValue = _elapsedTime / _lerpDuration;
            Context.CurrentSpeed = Mathf.Lerp(Context.CurrentSpeed, Context.TargetSpeed, lerpValue);

            Context.Animator.SetLayerWeight(1, lerpValue);
            Context.AimConstraint.weight = lerpValue;

            Context.CinemachineOrbitalFollow.Radius = Mathf.Lerp(Context.CinemachineOrbitalFollow.Radius, AimCenterRadius, lerpValue);
        }

        // Smooth input
        _smoothedMoveInput = Vector2.Lerp(
            _smoothedMoveInput,
            Context.MoveInput,
            Time.deltaTime * _inputLerpSpeed
        );

        // Apply smoothed animator parameters
        Context.Animator.SetFloat("Speed", Context.CurrentSpeed);
        Context.Animator.SetFloat("InputX", _smoothedMoveInput.x);
        Context.Animator.SetFloat("InputY", _smoothedMoveInput.y);

        UpdateHorizontalVelocity(Context.CurrentSpeed);
        ShootHandler();
        HandleHorizontalRotation();
    }



    void HandleHorizontalRotation()
    {
        Vector3 flatCamDir = GetFlatForward(Context.CameraTransform.forward);
        Vector3 flatRootForward = GetFlatForward(Context.RootTransform.forward);
        Context.LastMoveDirection = flatCamDir;
        float angleFromBaseline = Vector3.SignedAngle(currentFlatForward, flatCamDir, Vector3.up);

        // If outside limits, start turning
        if (!isTurningRoot && (angleFromBaseline < leftLimit || angleFromBaseline > rightLimit))
        {
            isTurningRoot = true;
        }

        if (isTurningRoot)
        {
            Vector3 targetForward = Vector3.RotateTowards(flatRootForward, flatCamDir, Time.deltaTime * rotationSpeed, 0f);
            Context.RootTransform.forward = targetForward;
            Context.AimConstraint.weight = 0.8f;

            // Once the root is close enough to camera direction, stop turning & update baseline
            float newAngle = Vector3.SignedAngle(flatRootForward, flatCamDir, Vector3.up);
            if (Mathf.Abs(newAngle) < 0.5f) // tweak tolerance
            {
                currentFlatForward = flatCamDir;
                isTurningRoot = false;
            }
        }
        else
        {
            Context.AimConstraint.weight = 1f;
        }
    }


    void ShootHandler()
    {
        // Always update cooldown timer
        timeSinceLastShot = Time.time - _lastShootTime;

        switch (Context.CurrentShootingMethod)
        {
            case PlayerContext.EShootingMethod.Single:
                ApplyShootMethodAnim("SingleShot");
                break;

            case PlayerContext.EShootingMethod.Burst:
                ApplyShootMethodAnim("BurstShot");
                break;

            case PlayerContext.EShootingMethod.Auto:
                Context.Animator.SetBool("IsAutoShot", Context.IsShooting);
                break;
        }
    }

    private void ApplyShootMethodAnim(string animName)
    {
        if (Context.IsShooting)
        {
            Context.IsShooting = false;
            if (timeSinceLastShot >= _shootCooldown)
            {
                Context.Animator.Play(animName,1);
                _lastShootTime = Time.time;
            }
        }
    }

    public override void FixedUpdate()
    {
        
    }
    public override void LateUpdate()
    {
        
    }
    public override void ExitState()
    {
        Context.Animator.SetLayerWeight(1, 0f);

        Context.Animator.Play("Aim", 1);
    }
    


    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        Context.NextAnimCrossFadeTime = 0.1f;
        if (!Context.IsAiming)
        {
            
            if (Context.MoveInput == Vector2.zero)
            {
                return PlayerStateMachine.EPlayerState.Idle;
            }
            else
            {
                return PlayerStateMachine.EPlayerState.Run;
            }
        }
        return StateKey;
    }


    public override void OnTriggerEnter(Collider collision)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTriggerExit(Collider collision)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTriggerStay(Collider collision)
    {
        throw new System.NotImplementedException();
    }
}
