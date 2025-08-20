using UnityEngine;

public class JumpState : PlayerState
{
    public JumpState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(context, stateKey)
    {
    }

    public override void EnterState()
    {
        Context.Animator.CrossFade("Jump",Context.NextAnimCrossFadeTime);
        Context.VelocityY = 3;
    }

    public override void UpdateState()
    {
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
        Context.IsJumping = false; // Reset jumping state when exiting
    }


    public override PlayerStateMachine.EPlayerState GetNextState()
    { 
        Context.NextAnimCrossFadeTime = 0.1f;
        if(Context.CharacterController.isGrounded)
        {
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
