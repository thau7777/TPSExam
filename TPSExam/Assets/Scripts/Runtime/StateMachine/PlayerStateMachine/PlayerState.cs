using UnityEngine;

public abstract class PlayerState : BaseState<PlayerStateMachine.EPlayerState>
{
    protected PlayerContext Context;
    public PlayerState(PlayerContext context,
        PlayerStateMachine.EPlayerState stateKey) : base(stateKey)
    {
        Context = context;
    }

    protected void UpdateHorizontalVelocity(float speedToMove, Vector3? directionToMove = null)
    {
        Vector3 camForward = Context.CameraTransform.forward;
        Vector3 camRight = Context.CameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveInputDirection = (camForward * Context.MoveInput.y + camRight * Context.MoveInput.x);

        Vector3 finalDirection;

        if (directionToMove == null) finalDirection = moveInputDirection.normalized;
        else finalDirection = (directionToMove.Value + moveInputDirection).normalized;

        if (finalDirection != Vector3.zero)
            Context.LastMoveDirection = finalDirection;

        Context.HorizontalVelocity = finalDirection * speedToMove;
    }
}
