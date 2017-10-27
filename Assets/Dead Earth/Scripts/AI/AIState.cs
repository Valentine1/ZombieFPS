using UnityEngine;
using System.Collections;

public enum AITriggerEventType {Enter, Stay, Exit } 

public abstract class AIState : MonoBehaviour
{

    protected AIStateMachine _stateMachine;

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    public void SetStateMachine(AIStateMachine machine)
    {
        _stateMachine = machine;
    }

    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnSensorEvent(AITriggerEventType eventType, Collider c) { }
    public virtual void OnDestinationReached(bool isReached) { }
}
