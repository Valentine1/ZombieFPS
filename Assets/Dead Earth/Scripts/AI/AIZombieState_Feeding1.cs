using UnityEngine;
using System.Collections;

public class AIZombieState_Feeding1 : AIZombieState {

    [SerializeField]
    float SlerpSpeed = 5.0f;


    private int EatingStateHash = Animator.StringToHash("FeedingState");
    private int EatingLayerIndex = -1;

    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    public override void OnEnterState()
    {
        Debug.Log("Enter feeding State");
        base.OnEnterState();

        if (this.ZombieStateMachine == null)
        {
            return;
        }
        if (this.EatingLayerIndex == -1)
        {
            this.EatingLayerIndex = this.ZombieStateMachine.BodyAnimator.GetLayerIndex("Cinematic");
        }

        //Configure State Machine   
        this.ZombieStateMachine.NavAgentControl(true, false);
        this.ZombieStateMachine.Feeding = true;
        this.ZombieStateMachine.Seeking = 0;
        this.ZombieStateMachine.Speed = 0;
        this.ZombieStateMachine.BodyNavAgent.speed = 0;
        this.ZombieStateMachine.AttackType = 0;
       
    }

    public override void OnExitState()
    {
        base.OnExitState();
        this.ZombieStateMachine.Feeding = false;
    }

    public override AIStateType OnUpdate()
    {
        if (this.ZombieStateMachine.Satisfaction > 0.9f)
        {
            this.ZombieStateMachine.GetNextWayPoint(false);
            return AIStateType.Alerted;
        }

        if (this.ZombieStateMachine.VisualTarget.Type != AITargetType.None && this.ZombieStateMachine.VisualTarget.Type != AITargetType.Food)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Alerted;
        }

        if (this.ZombieStateMachine.AudioTarget.Type  == AITargetType.Audio)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
            return AIStateType.Alerted;
        }

        if (this.ZombieStateMachine.BodyAnimator.GetCurrentAnimatorStateInfo(this.EatingLayerIndex).shortNameHash == this.EatingStateHash)
        {
            this.ZombieStateMachine.Satisfaction = Mathf.Min(this.ZombieStateMachine.Satisfaction + (Time.deltaTime * this.ZombieStateMachine.ReplenishRate)/100f, 1.0f);
        }

        if (!this.ZombieStateMachine.useRootRotation)
        {
            Vector3 targetPos = this.ZombieStateMachine.ActualTargetPosition;
            targetPos.y = this.ZombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - this.ZombieStateMachine.transform.position);
            this.ZombieStateMachine.transform.rotation =Quaternion.Slerp( this.ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * this.SlerpSpeed);
        }

        return AIStateType.Feeding;
    }
}
