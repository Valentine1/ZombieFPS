using UnityEngine;
using System.Collections;

public class AIStateMachineLink : StateMachineBehaviour
{
    protected AIStateMachine _stateMachine;
    public AIStateMachine StateMachine
    {
        set
        {
            _stateMachine = value;
        }
    }
	
}
