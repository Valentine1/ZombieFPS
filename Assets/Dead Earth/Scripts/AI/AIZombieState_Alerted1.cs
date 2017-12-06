using UnityEngine;
using System.Collections;

public class AIZombieState_Alerted1 : AIZombieState
{

    [SerializeField]
    [Range(5, 30)]
    float DurationInAlertedState = 10f;

    [SerializeField]
    float ReturnToPatrolAngleThreshold = 60f;
    [SerializeField]
    float ThreatDetectedAngleThreshold = 10f;
    [SerializeField]
    float DirectionChangeTime = 1.5f;

    private float timer = 0f;
    private float directionChangeTimer = 0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (this.ZombieStateMachine == null)
        {
            return;
        }

        //Configure State Machine   `
        this.timer = 0;
        this.directionChangeTimer = 0;
        this.ZombieStateMachine.NavAgentControl(true, false);
        this.ZombieStateMachine.Speed = 0;
        this.ZombieStateMachine.Seeking = 0;
        this.ZombieStateMachine.Feeding = false;
        this.ZombieStateMachine.AttackType = 0;

    }

    public override AIStateType OnUpdate()
    {
        this.timer += Time.deltaTime;
        this.directionChangeTimer += Time.deltaTime;

        if (this.timer > this.DurationInAlertedState)
        {
            this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.GetNextWayPoint(false));
            this.ZombieStateMachine.BodyNavAgent.Resume();
            this.timer = 0;
            return AIStateType.Patrol;
        }
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Player)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Pursuit;
        }
        if (this.ZombieStateMachine.AudioTarget.Type == AITargetType.Audio)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
            this.timer = 0;
        }
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Light)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            this.timer = 0;
        }
        if (this.ZombieStateMachine.AudioTarget.Type == AITargetType.None && 
            this.ZombieStateMachine.VisualTarget.Type == AITargetType.Food)
        {
            // only if zombie is hungry enough
            if ((1.0f - this.ZombieStateMachine.Satisfaction) > (this.ZombieStateMachine.VisualTarget.Distance / this.ZombieStateMachine.SensorRadius))
            {
                this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
                return AIStateType.Pursuit;
            }
        }

        float angleToTarget;
        if (this.ZombieStateMachine.ActualTargetType == AITargetType.Audio || this.ZombieStateMachine.ActualTargetType == AITargetType.Light)
        {
            angleToTarget = AIState.FindSignedAngle(this.ZombieStateMachine.transform.forward,
                                                      this.ZombieStateMachine.ActualTargetPosition - this.ZombieStateMachine.transform.position);
            if (this.ZombieStateMachine.ActualTargetType == AITargetType.Audio && Mathf.Abs(angleToTarget) < this.ThreatDetectedAngleThreshold)
            {
                if (!this.ZombieStateMachine.IsTargetReached)
                {
                    return AIStateType.Pursuit;
                }
              
            }

            if (this.directionChangeTimer > this.DirectionChangeTime)
            {
                if (Random.value < this.ZombieStateMachine.Intelligence)
                {
                    this.ZombieStateMachine.Seeking = (int)Mathf.Sign(angleToTarget);
                }
                else
                {
                    this.ZombieStateMachine.Seeking = (int)Mathf.Sign(Random.Range(-1f, 1f));
                }
                this.directionChangeTimer = 0f;
            }
          
        }

        if (this.ZombieStateMachine.ActualTargetType == AITargetType.Waypoint && !this.ZombieStateMachine.BodyNavAgent.pathPending)
        {
            angleToTarget = AIState.FindSignedAngle(this.ZombieStateMachine.transform.forward,
                                                     this.ZombieStateMachine.BodyNavAgent.steeringTarget - this.ZombieStateMachine.transform.position);
      
            if (Mathf.Abs(angleToTarget) < this.ReturnToPatrolAngleThreshold)
            {
                return AIStateType.Patrol;
            }
            this.ZombieStateMachine.Seeking = (int)Mathf.Sign(angleToTarget);
        }

        return AIStateType.Alerted;
    }
}
