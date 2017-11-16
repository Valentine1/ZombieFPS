using UnityEngine;
using System.Collections;

public class AIZombieState_Patrol1 : AIZombieState {

    //Inspector Assigned
    [SerializeField]
    AIWaypointNetwork PatrolNetwork = null;

    [SerializeField]
    bool RandomPatrol = false;

    [SerializeField]
    int WayPointIndex = 0;

    [SerializeField, Range(0f,3f)]
    float SpeedDescrete = 1.0f;

    [SerializeField]
    float TurnOnSpotAnimationThreshold = 80f;

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
        this.ZombieStateMachine.Speed = this.SpeedDescrete;
        this.ZombieStateMachine.BodyNavAgent.speed = this.SpeedDescrete;
        this.ZombieStateMachine.Seeking = 0;
        this.ZombieStateMachine.Feeding = false;
        this.ZombieStateMachine.AttackType = 0;

        //if targate type is waypoint then set next (or random) waypoint as a target 
        if (this.ZombieStateMachine.ActualTargetType == AITargetType.Waypoint)
        {
            this.ZombieStateMachine.ClearActualTarget();
            if (this.PatrolNetwork != null && this.PatrolNetwork.Waypoints.Count > 0)
            {
                if (this.RandomPatrol)
                {
                    this.WayPointIndex = Random.Range(0, this.PatrolNetwork.Waypoints.Count);
                }

                Transform waypoint =  this.PatrolNetwork.Waypoints[this.WayPointIndex];
                //setting the new StateMachine Actual Target
                this.ZombieStateMachine.SetActualTarget(AITargetType.Waypoint, null, waypoint.position,
                                                        Vector3.Distance(this.ZombieStateMachine.transform.position, waypoint.position));
                //Tell NavAgent to make a path to this waypoint
                this.ZombieStateMachine.BodyNavAgent.SetDestination(waypoint.position);
            }
        }

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

        if (this.ZombieStateMachine.BodyNavAgent.isPathStale || !this.ZombieStateMachine.BodyNavAgent.hasPath ||
            this.ZombieStateMachine.BodyNavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            CalculateNextWayPoint();
        }

        return AIStateType.Patrol;
    }


    private void CalculateNextWayPoint()
    {
        if (this.RandomPatrol && this.PatrolNetwork.Waypoints.Count > 1)
        {
            int oldWayPointIndex = this.WayPointIndex;
            while (this.WayPointIndex == oldWayPointIndex)
            {
                this.WayPointIndex = Random.Range(0, this.PatrolNetwork.Waypoints.Count);
            }
        }
        else
        {
            this.WayPointIndex = this.WayPointIndex == this.PatrolNetwork.Waypoints.Count - 1 ? 0 : this.WayPointIndex+1;
        }

        Transform newWayPoint = this.PatrolNetwork.Waypoints[this.WayPointIndex];
        this.ZombieStateMachine.SetActualTarget(AITargetType.Waypoint, null, newWayPoint.position,
                                                    Vector3.Distance(newWayPoint.position, this.ZombieStateMachine.transform.position));
        //Set NavAgent component destination
        this.ZombieStateMachine.BodyNavAgent.SetDestination(newWayPoint.position);
    }

    public override void OnDestinationReached(bool isReached)
    {
        base.OnDestinationReached(isReached);
        if (isReached && this.ZombieStateMachine.ActualTargetType == AITargetType.Waypoint)
        {
            CalculateNextWayPoint();
        }
    }

    public override void OnAnimatorIKUpdated()
    {
        base.OnAnimatorIKUpdated();
        if (this.ZombieStateMachine == null)
            return;
        this.ZombieStateMachine.BodyAnimator.SetLookAtPosition(this.ZombieStateMachine.ActualTargetPosition + Vector3.up);
        this.ZombieStateMachine.BodyAnimator.SetLookAtWeight(0.6f);
    }
}
