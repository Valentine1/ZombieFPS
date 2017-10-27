using UnityEngine;
using System.Collections;

public class AISensor : MonoBehaviour {

    private AIStateMachine _parentStateMachine = null;
    public AIStateMachine ParentStateMachine
    {
        set
        {
            _parentStateMachine = value;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        _parentStateMachine.OnSensorEvent(AITriggerEventType.Enter, col);
    }

    void OnTriggerStay(Collider col)
    {
        _parentStateMachine.OnSensorEvent(AITriggerEventType.Stay, col);
    }

    void OnTriggerExit(Collider col)
    {
        _parentStateMachine.OnSensorEvent(AITriggerEventType.Exit, col);
    }
}
