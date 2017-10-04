﻿using UnityEngine;
using System.Collections;

public class SmoothCameraMount : MonoBehaviour {

    public Transform Mount = null;
    public float Speed = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        this.transform.position = Vector3.Lerp(this.transform.position, Mount.position, 0.5f);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Mount.rotation, 0.5f);
	}
}
