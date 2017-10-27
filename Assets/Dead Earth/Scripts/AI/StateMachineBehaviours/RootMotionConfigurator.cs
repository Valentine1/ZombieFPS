using UnityEngine;
using System.Collections;

public class RootMotionConfigurator : AIStateMachineLink {

    [SerializeField]
    protected int _rootPosition = 0;

    [SerializeField]
    protected int _rootRotation = 0;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (this._stateMachine)
        {
            this._stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (this._stateMachine)
        {
            this._stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
        }
    }
}
