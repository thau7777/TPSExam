using UnityEngine;
using System;

public abstract class BaseState<EState> : IState<EState>
{
    public BaseState(EState key)
    {
        StateKey = key;
    }

    public EState StateKey { get; private set; }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void FixedUpdate();
    public abstract void LateUpdate();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter(Collider collision);
    public abstract void OnTriggerStay(Collider collision);
    public abstract void OnTriggerExit(Collider collision);
}
