using UnityEngine;

public class ReloadState : PlayerState
{
    Vector2 _smoothedMoveInput;
    float _inputLerpSpeed = 10f; // higher = faster response

    public float aimAngleThreshold = 60f;
    public float rotationSpeed = 5f;
    float leftLimit = -50f;   // customize
    float rightLimit = 75f;   // customize

    Vector3 currentFlatForward;
    bool isTurningRoot = false;
    public ReloadState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(context, stateKey)
    {
    }

    public override void EnterState()
    {
        Context.Animator.SetLayerWeight(1, 1f);
        Context.Animator.CrossFade("Strafe", Context.NextAnimCrossFadeTime);
        Context.Animator.CrossFade("Reload", Context.NextAnimCrossFadeTime);
    }

    public override void UpdateState()
    {

        // Smooth input
        _smoothedMoveInput = Vector2.Lerp(
            _smoothedMoveInput,
            Context.MoveInput,
            Time.deltaTime * _inputLerpSpeed
        );

        // Apply smoothed animator parameters
        Context.Animator.SetFloat("Speed", Context.TargetSpeed);
        Context.Animator.SetFloat("InputX", _smoothedMoveInput.x);
        Context.Animator.SetFloat("InputY", _smoothedMoveInput.y);

        UpdateHorizontalVelocity(Context.TargetSpeed);
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
            // Rotate root
            Vector3 targetForward = Vector3.RotateTowards(flatRootForward, flatCamDir, Time.deltaTime * rotationSpeed, 0f);
            Context.RootTransform.forward = targetForward;


            // Once close enough to camera direction, stop turning
            float newAngle = Vector3.SignedAngle(flatRootForward, flatCamDir, Vector3.up);
            if (Mathf.Abs(newAngle) < 0.5f)
            {
                currentFlatForward = flatCamDir;
                isTurningRoot = false;
            }
        }
        
    }

    Vector3 GetFlatForward(Vector3 forward)
    {
        forward.y = 0;
        return forward.normalized;
    }
    public override void FixedUpdate()
    {
    }
    public override void LateUpdate()
    {
    }
    public override void ExitState()
    {
        Context.IsReloading = false;
        Context.Animator.SetLayerWeight(1, 0f);
        Context.Animator.Play("Aim", 1);
    }


    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        Context.NextAnimCrossFadeTime = 0.1f;
        if (Context.IsAiming || !Context.IsReloading)
        {
            Context.IsAiming = false;
            if (Context.MoveInput != Vector2.zero)
            {
                return PlayerStateMachine.EPlayerState.Run;
            }
            else
            {
                return PlayerStateMachine.EPlayerState.Idle;
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
