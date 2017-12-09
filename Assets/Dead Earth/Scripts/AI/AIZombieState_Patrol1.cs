using UnityEngine;
using System.Collections;

public class AIZombieState_Patrol1 : AIZombieState {

    //Inspector Assigned
    [SerializeField, Range(0f,3f)]
    float SpeedDescrete = 2.0f;

    [SerializeField]
    float TurnOnSpotAnimationThreshold = 90f;

    [SerializeField]
    float SlerpSpeed = 5.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }


    public override void OnEnterState()
    {
        Debug.Log("Enter Patrol State");
        base.OnEnterState();
        if (this.ZombieStateMachine == null)
        {
            return;
        }

        //Configure State Machine   
        this.ZombieStateMachine.NavAgentControl(true, false);
        this.ZombieStateMachine.Seeking = 0;
      
        this.ZombieStateMachine.Feeding = false;
        this.ZombieStateMachine.AttackType = 0;

        this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.GetNextWayPoint(false));

        this.ZombieStateMachine.BodyNavAgent.Resume();
    }

    //called by StateMachine in its Update()
    public override AIStateType OnUpdate()
    {
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Player)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Pursuit;
        }
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Light)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Alerted;
        }
        if (this.ZombieStateMachine.AudioTarget.Type == AITargetType.Audio)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
            return AIStateType.Alerted;
        }
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Food)
        {
            // only if zombie is hungry enough
            if ((1.0f - this.ZombieStateMachine.Satisfaction) > (this.ZombieStateMachine.VisualTarget.Distance / this.ZombieStateMachine.SensorRadius))
            {
                this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
                return AIStateType.Pursuit;
            }
        }

        if (this.ZombieStateMachine.BodyNavAgent.pathPending)
        {
            this.ZombieStateMachine.Speed = 0f;
            this.ZombieStateMachine.BodyNavAgent.speed = 0f;
            return AIStateType.Patrol;
        }
        else
        {
            this.ZombieStateMachine.Speed = this.SpeedDescrete;
            this.ZombieStateMachine.BodyNavAgent.speed = this.SpeedDescrete;
        }

        float angleOfTurn = Vector3.Angle(this.StateMachine.transform.forward, this.StateMachine.BodyNavAgent.steeringTarget -
                                                                                     this.StateMachine.transform.position);
        if (angleOfTurn > this.TurnOnSpotAnimationThreshold)
        {
            return AIStateType.Alerted;
        }

        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!this.ZombieStateMachine.useRootRotation)
        {
           // Quaternion newRot = Quaternion.LookRotation(this.ZombieStateMachine.BodyNavAgent.steeringTarget);
            Quaternion lookRotation = Quaternion.LookRotation(this.ZombieStateMachine.BodyNavAgent.desiredVelocity, Vector3.up);
            this.ZombieStateMachine.transform.rotation = Quaternion.Slerp(this.ZombieStateMachine.transform.rotation, lookRotation, Time.deltaTime * this.SlerpSpeed);
                          
        }

        if (this.ZombieStateMachine.BodyNavAgent.remainingDistance <= this.ZombieStateMachine.BodyNavAgent.stoppingDistance ||
            this.ZombieStateMachine.BodyNavAgent.isPathStale || !this.ZombieStateMachine.BodyNavAgent.hasPath ||
            this.ZombieStateMachine.BodyNavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            this.ZombieStateMachine.GetNextWayPoint(true);
        }

        return AIStateType.Patrol;
    }


   

    public override void OnDestinationReached(bool isReached)
    {
        base.OnDestinationReached(isReached);
        if (isReached && this.ZombieStateMachine.ActualTargetType == AITargetType.Waypoint)
        {
            this.ZombieStateMachine.GetNextWayPoint(true);
        }
    }

    public override void OnAnimatorIKUpdated()
    {
        base.OnAnimatorIKUpdated();
        if (this.ZombieStateMachine == null)
            return;
        //this.ZombieStateMachine.BodyAnimator.SetLookAtPosition(this.ZombieStateMachine.ActualTargetPosition + Vector3.up);
        //this.ZombieStateMachine.BodyAnimator.SetLookAtWeight(0.6f);
    }
}
