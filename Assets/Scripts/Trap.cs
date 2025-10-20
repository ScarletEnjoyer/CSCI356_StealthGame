using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

	void Start () {
        Invoke("Timeup", 60);
	}

    void Timeup()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        print("trap!");
        var go = other.gameObject;
        if (go.tag == "Enemy")
        {
            go.GetComponent<EnemyCharacter>().OnTrap(15);
            Destroy(gameObject);
        }
    }

}
