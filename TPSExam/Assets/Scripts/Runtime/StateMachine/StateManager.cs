using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    protected bool IsTransitioningState = false;

    public event Action<EState> OnStateChanged;

    protected virtual void OnEnable()
    {
        OnStateChanged += TransitionToState;
    }

    protected virtual void OnDisable()
    {
        OnStateChanged -= TransitionToState;
    }

    private void Start() 
    {
        CurrentState.EnterState();
    }

    protected virtual void Update() 
    {
        EState nextStateKey = CurrentState.GetNextState();
        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if (!IsTransitioningState)
        {
            OnStateChanged?.Invoke(nextStateKey);
        }
    }

    private void FixedUpdate()
    {
        CurrentState.FixedUpdate();
    }

    private void LateUpdate()
    {
        CurrentState.LateUpdate();
    }

    private void OnTriggerEnter(Collider other) 
    {
        CurrentState.OnTriggerEnter(other);
    }

    private void OnTriggerStay(Collider other) 
    {
        CurrentState.OnTriggerStay(other);
    }

    private void OnTriggerExit(Collider other) 
    {
        CurrentState.OnTriggerExit(other);
    }

    private void TransitionToState(EState nextStateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[nextStateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }
}
