using UnityEngine;
using System.Collections;

public class MoveController : MonoBehaviour {

    private Animator _animator = null;
    private int SideMoveHash;
    private int ForwardMoveHash;
    private int IsAttackingHash;

	// Use this for initialization
	void Start () {
        _animator = this.GetComponent<Animator>();
        SideMoveHash = Animator.StringToHash("SideMove");
        ForwardMoveHash = Animator.StringToHash("ForwardMove");
        IsAttackingHash = Animator.StringToHash("IsAttacking");
    }
	
	// Update is called once per frame
	void Update () {

        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.66f;
        _animator.SetFloat(SideMoveHash, xAxis, 0.2f, Time.deltaTime);
        _animator.SetFloat(ForwardMoveHash, yAxis, 0.3f, Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger(IsAttackingHash);
        }
	}
}
