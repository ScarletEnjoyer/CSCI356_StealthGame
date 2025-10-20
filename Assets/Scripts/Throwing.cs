using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwing : MonoBehaviour {

    public Vector3 throwTarget;
    float time;
    float speedParam = 20;
    Vector3 startPos;
    float vx, vz, a, vy, T;

    bool free;

    Rigidbody rigid;
    Collider coll;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();

        startPos = transform.position;
        Vector3 to = throwTarget - startPos;
        float d = to.magnitude;
        T = d / speedParam;
        float h = d * 0.4f;

        a = 2.0f * h / (T * T / 4.0f);
        vy = a * (T / 2);

        vx = to.x / T;
        vz = to.z / T;

        free = false;
    }

    void Update ()
    {
        if (free)
        {
            //transform.position = info.target;
            return;
        }

        transform.Rotate(0, 3, 0);

        float dt = Time.deltaTime;
        time += dt;
        transform.position += new Vector3(vx, vy, vz) * dt;

        vy -= a * dt;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            return;
        }
        if (free) return;
        free = true;

        coll.isTrigger = false;
        rigid.isKinematic = false;
    }

    public void DelayDestroy()
    {
        Destroy(gameObject, 10);
    }
}
