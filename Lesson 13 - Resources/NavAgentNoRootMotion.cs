using UnityEngine;
using System.Collections;

// ----------------------------------------------------------
// CLASS	:	NavAgentNoRootMotion
// DESC		:	Behaviour to test Unity's NavMeshAgent with
//				Animator component NOT using root motion
// ----------------------------------------------------------
[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour 
{
	// Inspector Assigned Variable
	public AIWaypointNetwork WaypointNetwork = null;
	public int				 CurrentIndex	 = 0;
	public bool				 HasPath		 = false;
	public bool				 PathPending	 = false;
	public bool				 PathStale		 = false;
	public NavMeshPathStatus PathStatus      = NavMeshPathStatus.PathInvalid;
	public AnimationCurve	 JumpCurve		 = new AnimationCurve();

	// Private Members
	private NavMeshAgent _navAgent 			= null;
	private Animator     _animator 			= null;
	private float		_originalMaxSpeed 	= 0;

	// -----------------------------------------------------
	// Name :	Start
	// Desc	:	Cache MavMeshAgent and set initial 
	//			destination.
	// -----------------------------------------------------
	void Start () 
	{
		// Cache NavMeshAgent Reference
		_navAgent = GetComponent<NavMeshAgent>();
		_animator = GetComponent<Animator>();

		if (_navAgent)
			_originalMaxSpeed = _navAgent.speed;

		// Turn off auto-update
		/*_navAgent.updatePosition = false;
		_navAgent.updateRotation = false;*/

		// If not valid Waypoint Network has been assigned then return
		if (WaypointNetwork==null) return;

		// Set first waypoint
		SetNextDestination ( false );
	}

	// -----------------------------------------------------
	// Name	:	SetNextDestination
	// Desc	:	Optionally increments the current waypoint
	//			index and then sets the next destination
	//			for the agent to head towards.
	// -----------------------------------------------------
	void SetNextDestination ( bool increment )
	{
		// If no network return
		if (!WaypointNetwork) return;

		// Calculatehow much the current waypoint index needs to be incremented
		int incStep = increment?1:0;
		Transform nextWaypointTransform = null;

		// Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
		int nextWaypoint = (CurrentIndex+incStep>=WaypointNetwork.Waypoints.Count)?0:CurrentIndex+incStep;
		nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

		// Assuming we have a valid waypoint transform
		if (nextWaypointTransform!=null)
		{
			// Update the current waypoint index, assign its position as the NavMeshAgents
			// Destination and then return
			CurrentIndex = nextWaypoint;
			_navAgent.destination = nextWaypointTransform.position;
			return;
		}

		// We did not find a valid waypoint in the list for this iteration
		CurrentIndex=nextWaypoint;
	}

	// ---------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called each frame by Unity
	// ---------------------------------------------------------
	void Update () 
	{
		int turnOnSpot;

		// Copy NavMeshAgents state into inspector visible variables
		HasPath 	= _navAgent.hasPath;
		PathPending = _navAgent.pathPending;
		PathStale	= _navAgent.isPathStale;
		PathStatus	= _navAgent.pathStatus;

		// Perform corss product on forard vector and desired velocity vector. If both inputs are Unit length
		// the resulting vector's magnitude will be Sin(theta) where theta is the angle between the vectors.
		Vector3 cross = Vector3.Cross( transform.forward, _navAgent.desiredVelocity.normalized);

		// If y component is negative it is a negative rotation else a positive rotation
		float horizontal = (cross.y<0)? -cross.magnitude : cross.magnitude;

		// Scale into the 2.32 range for our animator
		horizontal = Mathf.Clamp( horizontal * 2.32f, -2.32f, 2.32f );

		// If we have slowed down and the angle between forward vector and desired vector is greater than 10 degrees 
		if ( _navAgent.desiredVelocity.magnitude< 1.0f && Vector3.Angle( transform.forward, _navAgent.steeringTarget - transform.position) > 10.0f )
		{
			// Stop the nav agent (approx) and assign either -1 or +1 to turnOnSpot based on sign on horizontal
			_navAgent.speed = 0.1f;
			turnOnSpot 		= (int)Mathf.Sign( horizontal );
		}
		else
		{
			// Otherwise it is a small angle so set Agent's speed to normal and reset turnOnSpot
			_navAgent.speed = _originalMaxSpeed;
			turnOnSpot = 0;
		}

		// Send the data calculated above into the animator parameters
		_animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime );
		_animator.SetFloat("Vertical", _navAgent.desiredVelocity.magnitude , 0.1f, Time.deltaTime); 
		_animator.SetInteger("TurnOnSpot", turnOnSpot );

		// If agent is on an offmesh link then perform a jump
		/*if (_navAgent.isOnOffMeshLink)
		{
			StartCoroutine( Jump( 1.0f) );
			return;
		}*/

		// If we don't have a path and one isn't pending then set the next
		// waypoint as the target, otherwise if path is stale regenerate path
		if ( ( _navAgent.remainingDistance<=_navAgent.stoppingDistance && !PathPending) || PathStatus==NavMeshPathStatus.PathInvalid /*|| PathStatus==NavMeshPathStatus.PathPartial*/)
		{
			SetNextDestination ( true );
		}
			else
		if (_navAgent.isPathStale)
			SetNextDestination ( false );
	}

	// ---------------------------------------------------------
	// Name	:	Jump
	// Desc	:	Manual OffMeshLInk traversal using an Animation
	//			Curve to control agent height.
	// ---------------------------------------------------------
	IEnumerator Jump ( float duration )
	{
		// Get the current OffMeshLink data
		OffMeshLinkData 	data 		= _navAgent.currentOffMeshLinkData;

		// Start Position is agent current position
		Vector3				startPos	= _navAgent.transform.position;

		// End position is fetched from OffMeshLink data and adjusted for baseoffset of agent
		Vector3				endPos		= data.endPos + ( _navAgent.baseOffset * Vector3.up);

		// Used to keep track of time
		float 				time		= 0.0f;

		// Keeo iterating for the passed duration
		while ( time<= duration )
		{
			// Calculate normalized time
			float t = time/duration;

			// Lerp between start position and end position and adjust height based on evaluation of t on Jump Curve
			_navAgent.transform.position = Vector3.Lerp( startPos, endPos, t ) + (JumpCurve.Evaluate(t) * Vector3.up) ;

			// Accumulate time and yield each frame
			time += Time.deltaTime;
			yield return null;
		}

		// NOTE : Added this for a bit of stability to make sure the
		//        Agent is EXACTLY on the end position of the off mesh
		//		  link before completeing the link.
		_navAgent.transform.position = endPos;

		// All done so inform the agent it can resume control
		_navAgent.CompleteOffMeshLink();
	}
}
