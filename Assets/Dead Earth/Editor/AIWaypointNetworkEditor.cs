using UnityEngine;
using System.Collections;
using UnityEditor;

// ------------------------------------------------------------------------------------
// CLASS	:	AIWaypointNetworkEditor
// DESC		:	Custom Inspector and Scene View Rendering for the AIWaypointNetwork
//				Component
// ------------------------------------------------------------------------------------
[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor 
{
	// --------------------------------------------------------------------------------
	// Name	:	OnInspectorGUI (Override)
	// Desc	:	Called by Unity Editor when the Inspector needs repainting for an
	//			AIWaypointNetwork Component
	// --------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
        //base.OnInspectorGUI();
        // Get reference to selected component
        AIWaypointNetwork network = (AIWaypointNetwork)target;
	
        // Show the Display Mode Enumeration Selector
        network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode", network.DisplayMode);
	
        // If we are in Paths display mode then display the integer sliders for the Start and End waypoint indices
        if (network.DisplayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Waypoint Start", network.UIStart, 0, network.Waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Waypoint End", network.UIEnd, 0, network.Waypoints.Count - 1);
        }

        //// Tell Unity to do its default drawing of all serialized members that are NOT hidden in the inspector
        DrawDefaultInspector();
	}


	// --------------------------------------------------------------------------------
	// Name	:	OnSceneGUI
	// Desc	:	Implementing this functions means the Unity Editor will call it when
	//			the Scene View is being repainted. This gives us a hook to do our
	//			own rendering to the scene view.
	// --------------------------------------------------------------------------------
	void OnSceneGUI()
	{
		// Get a reference to the component being rendered
		AIWaypointNetwork network = (AIWaypointNetwork)target;
  
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white; 
		// Fetch all waypoints from the network and render a label for each one
		for(int i=0; i<network.Waypoints.Count;i++)
		{
			if (network.Waypoints[i]!=null)
                Handles.Label(network.Waypoints[i].position, "Waypoint " + i.ToString(), style);

		}

        //// If we are in connections mode then we will to draw lines
        //// connecting all waypoints
        if (network.DisplayMode == PathDisplayMode.Connections)
        {
            //    // Allocate array of vector to store the polyline positions
            Vector3[] linePoints = new Vector3[network.Waypoints.Count + 1];

            // Loop through each waypoint + one additional interation
            for (int i = 0; i <= network.Waypoints.Count; i++)
            {
                // Calculate the waypoint index with wrap-around in the
                // last loop iteration
                int index = i != network.Waypoints.Count ? i : 0;

                // Fetch the position of the waypoint for this iteration and
                // copy into our vector array.
                if (network.Waypoints[index] != null)
                    linePoints[i] = network.Waypoints[index].position;
                else
                    linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }

            //    // Set the Handle color to Cyan
            Handles.color = Color.cyan;

            //    // Render the polyline in the scene view by passing in our list of waypoint positions
            Handles.DrawPolyLine(linePoints);
        }
        else
        //// We are in paths mode so to proper navmesh path search and render result
        if (network.DisplayMode == PathDisplayMode.Paths)
        {
            // Allocate a new NavMeshPath
            NavMeshPath path = new NavMeshPath();

            // Assuming both the start and end waypoint indices selected are ligit
            if (network.Waypoints[network.UIStart] != null && network.Waypoints[network.UIEnd] != null)
            {
                // Fetch their positions from the waypoint network
                Vector3 from = network.Waypoints[network.UIStart].position;
                Vector3 to = network.Waypoints[network.UIEnd].position;
               
                // Request a path search on the nav mesh. This will return the path between
                // from and to vectors
                NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);

                // Set Handles color to Yellow
                Handles.color = Color.yellow;

                // Draw a polyline passing int he path's corner points
                Handles.DrawPolyLine(path.corners);
            }
        }
		
	}

}
