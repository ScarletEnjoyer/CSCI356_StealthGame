using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour {

    public Transform target;
    Vector3 offsetHigh;
    Vector3 offsetLow;
    Quaternion quatHigh;
    Quaternion quatLow;

    float lerp = 0f;

	void Start () {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        Transform high = GameObject.Find("CamPosHigh").transform;
        Transform low = GameObject.Find("CamPosLow").transform;

        offsetHigh = high.position - target.position;
        offsetLow = low.position - target.position;

        quatHigh = high.rotation;
        quatLow = low.rotation;
	}
	

	void Update () {
        float scroll = Input.mouseScrollDelta.y;
        lerp -= scroll * 0.05f;
        lerp = Mathf.Clamp(lerp, 0f, 1f);

        Vector3 offset = Vector3.Lerp(offsetHigh, offsetLow, lerp);
        Quaternion q = Quaternion.Slerp(quatHigh, quatLow, lerp);

        Vector3 pos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, pos, 0.5f);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, 0.5f);

	}
}
