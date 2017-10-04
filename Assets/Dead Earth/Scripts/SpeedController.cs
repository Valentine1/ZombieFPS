using UnityEngine;
using System.Collections;

public class SpeedController : MonoBehaviour {


    public float Speed = 0f;
   
    private Animator _animContr = null;

	// Use this for initialization
	void Start () {
        _animContr = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        _animContr.SetFloat("Speed", Speed);
	}
}
