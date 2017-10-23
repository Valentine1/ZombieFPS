using UnityEngine;
using System.Collections;

// ----------------------------------------------------------
// CLASS	:	NavAgentExample
// DESC		:	Behaviour to test Unity's NavMeshAgent
// ----------------------------------------------------------
[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{
    // Inspector Assigned Variable
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();

    // Private Members
    private NavMeshAgent _navAgent = null;
    private Animator _animator = null;
    private float originalNavMaxSpeed = 0;
    // -----------------------------------------------------
    // Name :	Start
    // Desc	:	Cache MavMeshAgent and set initial 
    //			destination.
    // -----------------------------------------------------
    void Start()
    {
        // Cache NavMeshAgent Reference
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = this.GetComponent<Animator>();
        originalNavMaxSpeed = _navAgent.speed;

        // Turn off auto-update
        /*_navAgent.updatePosition = false;
        _navAgent.updateRotation = false;*/


        // If not valid Waypoint Network has been assigned then return
        if (WaypointNetwork == null) return;

        // Set first waypoint
        SetNextDestination(false);
    }

    // -----------------------------------------------------
    // Name	:	SetNextDestination
    // Desc	:	Optionally increments the current waypoint
    //			index and then sets the next destination
    //			for the agent to head towards.
    // -----------------------------------------------------
    void SetNextDestination(bool increment)
    {
        // If no network return
        if (!WaypointNetwork) return;

        // Calculatehow much the current waypoint index needs to be incremented
        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        // Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
        int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

        // Assuming we have a valid waypoint transform
        if (nextWaypointTransform != null)
        {
            // Update the current waypoint index, assign its position as the NavMeshAgents
            // Destination and then return
            CurrentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }

        // We did not find a valid waypoint in the list for this iteration
        CurrentIndex = nextWaypoint;
    }

    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	Called each frame by Unity
    // ---------------------------------------------------------
    void Update()
    {
        int turnOnSpot = 0;

        // Copy NavMeshAgents state into inspector visible variables
        HasPath = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        PathStatus = _navAgent.pathStatus;

        Vector3 cross = Vector3.Cross(this.transform.forward, _navAgent.desiredVelocity.normalized);
        float sideMove = (cross.y < 0) ? -cross.magnitude : cross.magnitude;
        
        //If agents speed drops significantly (and desired angle of movemnet differs from current significanly)
        //it means agent is about to turn on spot.
        //Then we freeze navMesh movement temporarly to play "turn around" animation
        if (_navAgent.desiredVelocity.magnitude < 1f && Vector3.Angle(transform.forward, _navAgent.desiredVelocity) > 45)
        {
            _navAgent.speed = 0.11f;
            turnOnSpot = (int)Mathf.Sign(sideMove);
        }
        else
        {
            _navAgent.speed = originalNavMaxSpeed;
            turnOnSpot = 0;
        }

        sideMove = Mathf.Clamp(sideMove, -2.32f, 2.32f);
        _animator.SetFloat("SideMove", sideMove, 0.1f,Time.deltaTime);
        _animator.SetFloat("ForwardMove", _navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetInteger("TurnOnSpot", turnOnSpot);


        // If agent is on an offmesh link then perform a jump
        //if (_navAgent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(1.0f));
        //    return;
        //}

        // If we don't have a path and one isn't pending then set the next
        // waypoint as the target, otherwise if path is stale regenerate path
        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid /*|| PathStatus==NavMeshPathStatus.PathPartial*/)
        {
            SetNextDestination(true);
        }
        else
            if (_navAgent.isPathStale)
                SetNextDestination(false);
    }

    // ---------------------------------------------------------
    // Name	:	Jump
    // Desc	:	Manual OffMeshLInk traversal using an Animation
    //			Curve to control agent height.
    // ---------------------------------------------------------
    IEnumerator Jump(float duration)
    {
        // Get the current OffMeshLink data
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;

        // Start Position is agent current position
        Vector3 startPos = _navAgent.transform.position;

        // End position is fetched from OffMeshLink data and adjusted for baseoffset of agent
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);

        // Used to keep track of time
        float time = 0.0f;

        // Keeo iterating for the passed duration
        while (time <= duration)
        {
            // Calculate normalized time
            float t = time / duration;

            // Lerp between start position and end position and adjust height based on evaluation of t on Jump Curve
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);

            // Accumulate time and yield each frame
            time += Time.deltaTime;
            yield return null;
        }

        // All done so inform the agent it can resume control
        _navAgent.CompleteOffMeshLink();
    }
}
