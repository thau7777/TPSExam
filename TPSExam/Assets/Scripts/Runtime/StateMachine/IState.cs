using UnityEngine;

public interface IState<EState>
{
    void EnterState();
    void ExitState();
    void UpdateState();
    void FixedUpdate();
    void LateUpdate();
    EState GetNextState();
    void OnTriggerEnter(Collider collision);
    void OnTriggerStay(Collider collision);
    void OnTriggerExit(Collider collision);
}