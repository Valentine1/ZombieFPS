using UnityEngine;
using System.Collections;

public class AIZombieState_Idle1 : AIZombieState {

    //Inspector Assigned
    [SerializeField]
    Vector2 IdleTimeRange = new Vector2(10f, 60f);

    //Private
    float idleTime = 0f;
    float timer = 0f;

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (this.ZombieStateMachine == null)
        {
            return;
        }
        //Set how long in Idle state
        this.idleTime = Random.Range(this.IdleTimeRange.x, this.IdleTimeRange.y);
        this.timer = 0f;

        //Configure State Machine   `
        this.ZombieStateMachine.NavAgentControl(true, false);
        this.ZombieStateMachine.Speed = 0;
        this.ZombieStateMachine.Seeking = 0;
        this.ZombieStateMachine.Feeding = false;
        this.ZombieStateMachine.AttackType = 0;
        this.ZombieStateMachine.ClearActualTarget();
    }

    public override void OnExitState()
    {
        base.OnExitState();
    }

    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    //this is not Unity Update(), it is called from StateMachine
    public override AIStateType OnUpdate()
    {
        if (this.ZombieStateMachine == null)
        {
            return AIStateType.Idle;
        }
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
        if (this.ZombieStateMachine.AudioTarget.Type == AITargetType.Audio )
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.AudioTarget);
            return AIStateType.Alerted;
        }
        if (this.ZombieStateMachine.VisualTarget.Type == AITargetType.Food)
        {
            this.ZombieStateMachine.SetActualTarget(this.ZombieStateMachine.VisualTarget);
            return AIStateType.Pursuit;
        }
        this.timer += Time.deltaTime;
        if (this.timer > this.idleTime)
        {
            return AIStateType.Patrol;
        }

        return AIStateType.Idle;
    }
	
}
