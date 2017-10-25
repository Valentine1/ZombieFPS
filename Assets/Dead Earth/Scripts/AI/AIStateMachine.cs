using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AIStateType {None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType { None, Waypoint, Player, Light, Food, Audio }

public struct AITarget
{
    AITargetType _type;
    public AITargetType Type
    {
        get { return _type; }
    }

    Collider _collider;
    public Collider Collider
    {
        get { return _collider; }
    }
    
    Vector3 _position;
    public Vector3 Position
    {
        get { return _position; }
    }
    
    float _distance;
    public float Distance
    {
        get { return _distance; }
        set { _distance = value; }
    }
    
    float _lastPingTime;
    public float LastPingTime
    {
        get { return _lastPingTime; }
    }


    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _lastPingTime = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _lastPingTime = 0.0f;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    protected Dictionary<AIStateType, AIState> States = new Dictionary<AIStateType, AIState>();
    protected AITarget ActualTarget = new AITarget();
    protected AIState CurrentState = null;

    [SerializeField]
    protected AIStateType CurrentStateType = AIStateType.Idle;
    [SerializeField]
    protected SphereCollider TargetTrigger = null;
    [SerializeField]
    protected SphereCollider SensorTrigger = null;
    [SerializeField]
    [Range(0, 15)]
    protected float StoppingDistance = 1.0f;

    //Component Cache
    protected Animator _bodyAnimator = null;
    public Animator BodyAnimator
    {
        get { return _bodyAnimator; }
    }
    protected NavMeshAgent _bodyNavAgent = null;
    public NavMeshAgent BodyNavAgent
    {
        get { return _bodyNavAgent; }
    }
    protected Collider _bodyCollider = null;
    public Collider BodyCollider
    {
        get { return _bodyCollider; }
    }

    public AITarget VisualTarget = new AITarget();
    public AITarget AudioTarget = new AITarget();

    protected virtual void Awake()
    {
        _bodyAnimator = this.GetComponent<Animator>();
        _bodyNavAgent = this.GetComponent<NavMeshAgent>();
        _bodyCollider = this.GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        AIState[] states = this.GetComponents<AIState>();
        foreach (AIState state in states)
        {
            if(state!= null && States.ContainsKey(state.GetStateType()))
            {
             States[state.GetStateType()] = state;
             state.SetStateMachine(this);
            }
        }

        if (States.ContainsKey(CurrentStateType))
        {
            this.CurrentState = States[CurrentStateType];
            this.CurrentState.OnEnterState();
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        this.ActualTarget.Set(t, c, p, d);

        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.radius = this.StoppingDistance;
            this.TargetTrigger.transform.position = this.ActualTarget.Position;
            this.TargetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        this.ActualTarget = t;

        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.radius = this.StoppingDistance;
            this.TargetTrigger.transform.position = this.ActualTarget.Position;
            this.TargetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float stop)
    {
        this.ActualTarget.Set(t, c, p, d);

        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.radius = stop;
            this.TargetTrigger.transform.position = this.ActualTarget.Position;
            this.TargetTrigger.enabled = true;
        }
    }

    public void ClearActualTarget()
    {
        this.ActualTarget.Clear();
        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (this.CurrentState == null)
        {
            return;
        }
        AIStateType newStateType = this.CurrentState.OnUpdate();
        if (newStateType != this.CurrentStateType)
        {
            if (this.States.ContainsKey(newStateType))
            {
                this.CurrentState.OnExitState();
                this.States[newStateType].OnEnterState();
                this.CurrentState = this.States[newStateType];
                this.CurrentStateType = newStateType; 
            }
            else if (this.States.ContainsKey(AIStateType.Idle))
            {
                this.CurrentState.OnExitState();
                this.States[AIStateType.Idle].OnEnterState();
                this.CurrentState = this.States[AIStateType.Idle];
                this.CurrentStateType = AIStateType.Idle;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        this.VisualTarget.Clear();
        this.AudioTarget.Clear();

        if (this.ActualTarget.Type != AITargetType.None)
        {
            this.ActualTarget.Distance = Vector3.Distance(this.transform.position,this.ActualTarget.Position); 
        }
    }
}
