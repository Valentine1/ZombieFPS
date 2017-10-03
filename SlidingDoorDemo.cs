using UnityEngine;
using System.Collections;

// States the door can be in 
public enum DoorState { Open, Animating, Closed };

// -----------------------------------------------------------------------
// CLASS    :   SlidingDoorDemo
// DESC     :   Moves a game upject along its right vector by the 
//              specified distances over the specified duration.
//              The GameObject to which this is added should start
//              in the closed position
// ------------------------------------------------------------------------
public class SlidingDoorDemo : MonoBehaviour 
{
	// Public Members
	public	float			SlidingDistance	=	4.0f;
	public  float			Duration		=	1.5f;
	public  AnimationCurve	JumpCurve		=	new AnimationCurve();

	// Private Members
	private	Transform		_transform		=	null;
	private Vector3			_openPos		=	Vector3.zero;
	private Vector3			_closedPos		=	Vector3.zero;
	private DoorState		_doorState		=	DoorState.Closed;	


	// --------------------------------------------------------------------
    // Name :   Start
    // Desc :   Cache the transform component and the closed position.
    //          Calculate open position
    // --------------------------------------------------------------------
	void Start () 
	{
        // Cache transform and original position
		_transform 	= transform;
		_closedPos	= _transform.position;

        // Add right vector scaled by distance on to closed position to get
        // the open position
		_openPos	= _closedPos + ( _transform.right * SlidingDistance );
	}
	
	// --------------------------------------------------------------------
    // Name :   Update
    // Desc :   Called each frame by unity
    // --------------------------------------------------------------------
	void Update () 
	{
        // If the space bar is pressed and the door is not already animating
        // then start the coroutine to animate it
		if (Input.GetKeyDown( KeyCode.Space ) && _doorState!=DoorState.Animating)
		{
			StartCoroutine( AnimateDoor( (_doorState==DoorState.Open)?DoorState.Closed:DoorState.Open ) );
		}
	}

    // ---------------------------------------------------------------------
    // Name :   AnimateDoor (Coroutine)
    // Desc :   Animates the door into a new state over the Duration
    // ---------------------------------------------------------------------
	IEnumerator AnimateDoor ( DoorState newState )
	{
        // Block coroutine from starting again while it is still executing
		_doorState = DoorState.Animating;

        // Set timer to zero seconds
		float time = 0.0f;

        // Choose the starting position and ending positions of the Lerp
        // based on the state we are moving into 
		Vector3 startPos = (newState==DoorState.Open)?_closedPos:_openPos;
		Vector3 endPos	 = (newState==DoorState.Open)?_openPos:_closedPos;

        // Iterate for the duration
		while ( time <= Duration)
		{
            // Calculate normalized time and evaluate the result on our animation curve.
            // The result of the curve evaluation is then used as the t value in the
            // Vector Lerp between the start and ending positions
			float t = time/Duration;
			_transform.position = Vector3.Lerp(startPos, endPos, JumpCurve.Evaluate(t)) ;
			
            // Accumulate time and yield until the next frame
            time+=Time.deltaTime;
			yield return null;
		}

        // Snap object to the end position (just to make sure)
		_transform.position = endPos;

        // Assign new state to door
		_doorState = newState;
	}
}
