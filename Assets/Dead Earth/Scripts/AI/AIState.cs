using UnityEngine;
using System.Collections;

public enum AITriggerEventType {Enter, Stay, Exit } 

public abstract class AIState : MonoBehaviour
{
    protected AIStateMachine StateMachine;

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    public virtual void SetStateMachine(AIStateMachine machine)
    {
        StateMachine = machine;
    }

    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() 
    {
        if (StateMachine.useRootPosition)
        {
            // Override Agent's velocity with the velocity of the root motion
            StateMachine.BodyNavAgent.velocity = StateMachine.BodyAnimator.deltaPosition / Time.deltaTime;
        }
        if (StateMachine.useRootRotation)
        {
            //apply root motion rotation manually (Animator would apply it automatically if we had not define OnAnimatorUpdated)
            StateMachine.transform.rotation = StateMachine.BodyAnimator.rootRotation;
        }
    }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnSensorEvent(AITriggerEventType eventType, Collider c) 
    {
        if (this.StateMachine == null)
        {
            return;
        }
    }
    public virtual void OnDestinationReached(bool isReached) { }
}
