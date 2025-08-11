using UnityEngine;

public class ReloadState : PlayerState
{
    public ReloadState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(context, stateKey)
    {
    }

    public override void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }
    public override void FixedUpdate()
    {
        throw new System.NotImplementedException();
    }
    public override void LateUpdate()
    {
        throw new System.NotImplementedException();
    }
    public override void ExitState()
    {
        throw new System.NotImplementedException();
    }


    public override PlayerStateMachine.EPlayerState GetNextState()
    {
        throw new System.NotImplementedException();
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
