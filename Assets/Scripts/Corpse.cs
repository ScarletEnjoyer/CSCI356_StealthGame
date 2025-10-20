using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour {

    public void DelayDestroy()
    {
        Destroy(transform.parent.gameObject, 10);
    }
}
