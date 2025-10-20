using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleVehicle : MonoBehaviour {

    NavMeshAgent agent;

    public List<Transform> patrolPoints;
    int patrolIndex;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolIndex = 0;
        agent.isStopped = true;
        //foreach(Transform t in patrolPoints)
        //{
        //    print(t.position);
        //}
    }

    void UpdatePatrol()
    {
        if (patrolPoints.Count == 0)
        {
            return;
        }

        if (agent.isStopped)
        {
            agent.SetDestination(patrolPoints[patrolIndex].position);

            agent.isStopped = false;
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (patrolPoints.Count == 1)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, patrolPoints[0].rotation, 0.3f);
                return;
            }
            patrolIndex++;
            patrolIndex = patrolIndex % patrolPoints.Count;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Update()
    {
        UpdatePatrol();
    }
}
