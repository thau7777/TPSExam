using UnityEngine;

public class RunState : PlayerState
{
    float _elapsedTime = 0f;
    float _lerpDuration = 0.5f; // Time to decelerate to zero speed
    public RunState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(context, stateKey)
    {
    }

    public override void EnterState()
    {
        _elapsedTime = 0f;
        if (!Context.Animator.GetCurrentAnimatorStateInfo(0).IsName("GroundedMovement"))
            Context.Animator.CrossFade("GroundedMovement", Context.NextAnimCrossFadeTime);
    }

    public override void UpdateState()
    {
        if (_elapsedTime < _lerpDuration)
        {
            _elapsedTime += Time.deltaTime;
            float lerpValue = _elapsedTime / _lerpDuration;
            Context.CurrentSpeed = Mathf.Lerp(Context.CurrentSpeed, Context.TargetSpeed, lerpValue);
        }
        Context.Animator.SetFloat("Speed", Context.CurrentSpeed);

        UpdateHorizontalVelocity(Context.CurrentSpeed);
    }
    public override void FixedUpdate()
    {
        
    }
    public override void LateUpdate()
    {
        
    }
    public override void ExitState()
    {
        
    }


    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        Context.NextAnimCrossFadeTime = 0.1f;
        if (Context.MoveInput == Vector2.zero)
        {
            return PlayerStateMachine.EPlayerState.Idle;
        }
        if(Context.IsAiming)
        {
            return PlayerStateMachine.EPlayerState.Aim;
        }
        return StateKey;
    }


    public override void OnTriggerEnter(Collider collision)
    {
        
    }

    public override void OnTriggerExit(Collider collision)
    {
        
    }

    public override void OnTriggerStay(Collider collision)
    {
        
    }

}
