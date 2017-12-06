using UnityEngine;
using System.Collections;

public class AIZombieState_Pursuit1 : AIZombieState
{
    [SerializeField, Range(0f, 3f)]
    float SpeedDescrete = 3.0f;

    [SerializeField]
    float SlerpSpeed = 5.0f;

    [SerializeField]
    float RepathDistanceMultiplier = 0.035f; //time in which path is recalculating from zombie to Player

    [SerializeField]
    float RepathVisualMin = 0.05f; //time in which path is recalculating to Visual target

    [SerializeField]
    float RepathVisualMax = 5.0f;

    [SerializeField]
    float RepathAudioMin = 0.25f; //time in which path is recalculating to audio target (should be usuallly still)

    [SerializeField]
    float RepathAudioMax = 5.0f;

    [SerializeField]
    float PursuitDuration = 40.0f; //how long in pursuit before quitting 


    //internal variables
    private float repathTimer = 0.0f;
    private float pursuitTimer = 0.0f; //how long in pursuit

    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }
    // Use this for initialization
    void Start()
    {

    }

    public override void OnEnterState()
    {
        Debug.Log("Enter Pursuit State");
        base.OnEnterState();

        if (this.ZombieStateMachine == null)
        {
            return;
        }

        //Configure State Machine   
        this.ZombieStateMachine.NavAgentControl(true, false);
        this.ZombieStateMachine.Seeking = 0;
        this.ZombieStateMachine.Speed = this.SpeedDescrete;
        this.ZombieStateMachine.BodyNavAgent.speed = this.SpeedDescrete;
        this.ZombieStateMachine.Feeding = false;
        this.ZombieStateMachine.AttackType = 0;

        repathTimer = 0f;
        pursuitTimer = 0f;

        this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.ActualTargetPosition);
        this.ZombieStateMachine.BodyNavAgent.Resume();
    }

    public override AIStateType OnUpdate()
    {
        this.repathTimer += Time.deltaTime;
        this.pursuitTimer += Time.deltaTime;

        if (this.pursuitTimer > this.PursuitDuration)
        {
            return AIStateType.Patrol;
        }

        if (this.ZombieStateMachine.ActualTargetType == AITargetType.Player && this.ZombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }

        if (this.ZombieStateMachine.IsTargetReached)
        {
            #region
            switch (this.StateMachine.ActualTargetType)
            {
                case AITargetType.Audio:
                case AITargetType.Light:
                    this.StateMachine.ClearActualTarget();
                    return AIStateType.Alerted;
                case AITargetType.Food:
                    return AIStateType.Feeding;
            }
            #endregion
        }

        if (this.ZombieStateMachine.BodyNavAgent.isPathStale || !this.ZombieStateMachine.BodyNavAgent.hasPath ||
          this.ZombieStateMachine.BodyNavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }

        //very close to the player but not in mele range so "stick" zombie to face the player
        if (!this.ZombieStateMachine.useRootRotation && this.ZombieStateMachine.ActualTargetType == AITargetType.Player
            && this.ZombieStateMachine.VisualTarget.Type == AITargetType.Player && this.ZombieStateMachine.IsTargetReached)
        {
            #region
            Vector3 targetPos = this.ZombieStateMachine.ActualTargetPosition;
            targetPos.y = this.ZombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - this.ZombieStateMachine.transform.position);
            this.ZombieStateMachine.transform.rotation = newRot;
            #endregion
        }
        // far to the player, slowly ajust rotation to face player
        else if (!this.ZombieStateMachine.useRootRotation && !this.ZombieStateMachine.IsTargetReached)
        {
            #region
            Quaternion newRot = Quaternion.LookRotation(this.ZombieStateMachine.BodyNavAgent.desiredVelocity);

            this.ZombieStateMachine.transform.rotation = Quaternion.Slerp(this.ZombieStateMachine.transform.rotation, newRot, Time.deltaTime * this.SlerpSpeed);
            #endregion
        }
        else if (this.ZombieStateMachine.IsTargetReached)
        {
            return AIStateType.Alerted;
        }

        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Player)
        {
            #region
            //maybe we should recalculale the navigation path
            if (this.ZombieStateMachine.ActualTargetPosition != this.ZombieStateMachine.VisualTarget.Position)
            {
                if (Mathf.Clamp(this.ZombieStateMachine.VisualTarget.Distance * RepathDistanceMultiplier, RepathVisualMin, RepathVisualMax) < repathTimer)
                {
                    this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.VisualTarget.Position);
                    repathTimer = 0.0f;
                }
            }
            this.StateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Pursuit;
            #endregion
        }

        //Player is actual targte but not visual target so we continue pursuit to the last known position
        if (this.ZombieStateMachine.ActualTargetType == AITargetType.Player)
        {
            return AIStateType.Pursuit;
        }

        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Light)
        {
            #region
            // switch from less priority targets to the light
            if (this.ZombieStateMachine.ActualTargetType == AITargetType.Audio || this.ZombieStateMachine.ActualTargetType == AITargetType.Food)
            {
                this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
                return AIStateType.Alerted;
            }
            else if (this.ZombieStateMachine.ActualTargetType == AITargetType.Light)
            {
                //check if light is the same
                if (this.ZombieStateMachine.ActualTargetColliderID == this.ZombieStateMachine.VisualTarget.Collider.GetInstanceID())
                {
                    //check if light position has changed
                    if (this.ZombieStateMachine.ActualTargetPosition != this.ZombieStateMachine.VisualTarget.Position)
                    {
                        //recalculate path more frequently as getting closer
                        if (Mathf.Clamp(this.ZombieStateMachine.VisualTarget.Distance * this.RepathDistanceMultiplier, RepathVisualMin, RepathVisualMax) < repathTimer)
                        {
                            this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.VisualTarget.Position);
                            repathTimer = 0.0f;
                        }
                    }
                    this.StateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
                    return AIStateType.Pursuit;
                }
                else
                {
                    this.StateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
                    return AIStateType.Alerted;
                }
            }
            #endregion
        }
        else if(this.ZombieStateMachine.AudioTarget.Type == AITargetType.Audio)
        {
            #region
            // switch from less priority targets to the light
            if ( this.ZombieStateMachine.ActualTargetType == AITargetType.Food)
            {
                this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
                return AIStateType.Alerted;
            }
            else if (this.ZombieStateMachine.ActualTargetType == AITargetType.Audio)
            {
                //check if audio is the same
                if (this.ZombieStateMachine.ActualTargetColliderID == this.ZombieStateMachine.AudioTarget.Collider.GetInstanceID())
                {
                    if (this.ZombieStateMachine.ActualTargetPosition != this.ZombieStateMachine.AudioTarget.Position)
                    {
                        //recalculate path more frequently as getting closer
                        if (Mathf.Clamp(this.ZombieStateMachine.AudioTarget.Distance * this.RepathDistanceMultiplier, this.RepathAudioMin, this.RepathAudioMin) < repathTimer)
                        {
                            this.ZombieStateMachine.BodyNavAgent.SetDestination(this.ZombieStateMachine.AudioTarget.Position);
                            repathTimer = 0.0f;
                        }
                    }

                    this.StateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
                    return AIStateType.Pursuit;
                }
                else
                {
                    this.StateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
                    return AIStateType.Alerted;
                }
            }
            #endregion
        }



        return AIStateType.Pursuit;
    }
}
