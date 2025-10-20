using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour {

    Transform pole;
    Quaternion quatOpen = Quaternion.Euler(0, 0, 80);
    Quaternion quatClose = Quaternion.Euler(0, 0, 0);
    Quaternion quatTarget;

	void Start () {
        pole = transform.Find("pole");
        quatTarget = quatClose;
	}

    void Update()
    {
        Quaternion q = Quaternion.RotateTowards(pole.localRotation, quatTarget, 1);
        pole.localRotation = q;
    }

    public void OpenGate()
    {
        quatTarget = quatOpen;
    }

    public void CloseGate()
    {
        quatTarget = quatClose;
    }
	
}
