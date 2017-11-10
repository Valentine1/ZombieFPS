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
    protected int RootPositionRefCount = 0;
    protected int RootRotationRefCount = 0;

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

    public Vector3 SensorPosition
    {
        get
        {
            if (this.SensorTrigger == null)
            {
                return Vector3.zero;
            }
            Vector3 point = this.SensorTrigger.transform.position;
            point.x += this.SensorTrigger.center.x * this.SensorTrigger.transform.lossyScale.x;
            point.y += this.SensorTrigger.center.y * this.SensorTrigger.transform.lossyScale.y;
            point.z += this.SensorTrigger.center.z * this.SensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float SensorRadius
    {
        get
        {
            if (this.SensorTrigger == null)
            {
                return 0.0f;
            }
            float radius = Mathf.Max(this.SensorTrigger.radius * this.SensorTrigger.transform.lossyScale.x,
                                       this.SensorTrigger.radius * this.SensorTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, this.SensorTrigger.radius * this.SensorTrigger.transform.lossyScale.z);
        }
    }

    public AITarget VisualTarget = new AITarget();
    public AITarget AudioTarget = new AITarget();

    public bool useRootPosition { get { return this.RootPositionRefCount > 0; } }
    public bool useRootRotation { get { return this.RootRotationRefCount > 0; } }


    protected virtual void Awake()
    {
        _bodyAnimator = this.GetComponent<Animator>();
        _bodyNavAgent = this.GetComponent<NavMeshAgent>();
        _bodyCollider = this.GetComponent<Collider>();

        if (GameSceneManager.Instance != null)
        {
            if (_bodyCollider)
            {
                GameSceneManager.Instance.RegisterAIStateMachine(_bodyCollider.GetInstanceID(), this);
            }
            if (SensorTrigger)
            {
                GameSceneManager.Instance.RegisterAIStateMachine(SensorTrigger.GetInstanceID(), this);
            }
        }
    }

    protected virtual void Start()
    {
        if(this.SensorTrigger != null)
        {
            AISensor script = this.SensorTrigger.GetComponent<AISensor>();
            if (script != null)
            {
                script.ParentStateMachine = this;
            }
        }

        AIState[] states = this.GetComponents<AIState>();
        foreach (AIState state in states)
        {
            if(state!= null && !States.ContainsKey(state.GetStateType()))
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

        if (this.BodyAnimator)
        {
            AIStateMachineLink[] scripts = this.BodyAnimator.GetBehaviours<AIStateMachineLink>();

            foreach (AIStateMachineLink script in scripts)
            {
                script.StateMachine = this;
            }
        }
    }

    public void SetActualTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        this.ActualTarget.Set(t, c, p, d);

        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.radius = this.StoppingDistance;
            this.TargetTrigger.transform.position = this.ActualTarget.Position;
            this.TargetTrigger.enabled = true;
        }
    }

    public void SetActualTarget(AITarget t)
    {
        this.ActualTarget = t;

        if (this.TargetTrigger != null)
        {
            this.TargetTrigger.radius = this.StoppingDistance;
            this.TargetTrigger.transform.position = this.ActualTarget.Position;
            this.TargetTrigger.enabled = true;
        }
    }

    public void SetActualTarget(AITargetType t, Collider c, Vector3 p, float d, float stop)
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

    // --------------------------------------------------------------------------
    //	Name	:	OnTriggerEnter
    //	Desc	:	Called by Physics system when the AI's Main collider enters
    //				its trigger. This allows the child state to know when it has 
    //				entered the sphere of influence	of a waypoint or last player 
    //				sighted position.
    // --------------------------------------------------------------------------
    public virtual void OnTargetTriggerEnter(Collider col)
    {
        if (this.TargetTrigger == null || col != this.TargetTrigger)
        {
            return;
        }
        if (this.CurrentState != null)
        {
            this.CurrentState.OnDestinationReached(true);
        }
    }
    // --------------------------------------------------------------------------
    //	Name	:	OnTriggerExit
    //	Desc	:	Informs the child state that the AI entity is no longer at
    //				its destination (typically true when a new target has been
    //				set by the child.
    // --------------------------------------------------------------------------
    public virtual void OnTargetTriggerExit(Collider col)
    {
        if (this.TargetTrigger == null || col != this.TargetTrigger)
        {
            return;
        }
        if (this.CurrentState != null)
        {
            this.CurrentState.OnDestinationReached(false);
        }
    }

    // ------------------------------------------------------------
    // Name	:	OnSensorEvent
    // Desc	:	Called by our AISensor component when an AI Aggravator
    //			has entered/exited the sensor trigger.
    // -------------------------------------------------------------
    public virtual void OnSensorEvent(AITriggerEventType type, Collider col)
    {
        if (this.CurrentState != null)
        {
            this.CurrentState.OnSensorEvent(type, col);
        }
    }

    // -----------------------------------------------------------
    // Name	:	OnAnimatorMove
    // Desc	:	Called by Unity after root motion has been
    //			evaluated but not applied to the object.
    //			This allows us to determine via code what to do
    //			with the root motion information
    // -----------------------------------------------------------
    protected virtual void OnAnimatorMove()
    {
        if (this.CurrentState != null)
        {
            this.CurrentState.OnAnimatorUpdated();
        }
    }

    // ----------------------------------------------------------
    // Name	: OnAnimatorIK
    // Desc	: Called by Unity just prior to the IK system being
    //		  updated giving us a chance to setup up IK Targets
    //		  and weights.
    // ----------------------------------------------------------
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (this.CurrentState != null)
        {
            this.CurrentState.OnAnimatorIKUpdated();
        }
    }


    // ----------------------------------------------------------
    // Name	:	NavAgentControl
    // Desc	:	Configure the NavMeshAgent to enable/disable auto
    //			updates of position/rotation to our transform
    // ----------------------------------------------------------
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (this.BodyNavAgent != null)
        {
            this.BodyNavAgent.updatePosition = positionUpdate;
            this.BodyNavAgent.updateRotation = rotationUpdate;
        }
    }

    // ----------------------------------------------------------
    // Name	:	AddRootMotionRequest
    // Desc	:	Called by the State Machine Behaviours to
    //			Enable/Disable root motion
    // ----------------------------------------------------------
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        this.RootPositionRefCount += rootPosition;
        this.RootRotationRefCount += rootRotation;
    }


}
