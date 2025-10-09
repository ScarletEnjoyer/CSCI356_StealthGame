using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] waypoints; // �����GameObject���飨�ؿ���2,3,4��
    public float detectionRange = 10f; // ��ⷶΧ
    public float alertDuration = 5f; // ����ʱ��
    public Transform playerTarget; // ������Ҷ���
    public float viewAngle = 90f; // ��Ұ׶�Ƕ�

    private NavMeshAgent navAgent;
    private enum AIState { Patrol, Alert, Chase };
    private AIState currentState = AIState.Patrol;
    private int waypointIndex = 0;
    private float alertTimer = 0f;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (waypoints.Length > 0) navAgent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                if (navAgent.remainingDistance < 0.5f && !navAgent.pathPending)
                {
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    navAgent.SetDestination(waypoints[waypointIndex].position);
                }
                if (PlayerInLineOfSight()) SwitchState(AIState.Alert); // �޸����������FOV��鴥��Alert
                break;
            case AIState.Alert:
                navAgent.isStopped = true;
                alertTimer += Time.deltaTime;
                if (alertTimer >= alertDuration)
                {
                    alertTimer = 0f;
                    SwitchState(AIState.Patrol);
                }
                if (PlayerInLineOfSight()) SwitchState(AIState.Chase);
                break;
            case AIState.Chase:
                navAgent.isStopped = false;
                navAgent.SetDestination(playerTarget.position);
                if (!PlayerInLineOfSight()) SwitchState(AIState.Alert); // Chase��AlertҲ��FOV�����ⶪʧ��������Patrol
                break;
        }
    }

    bool PlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, playerTarget.position) <= detectionRange;
    }

    bool PlayerInLineOfSight()
    {
        if (!PlayerInDetectionRange()) return false; // �ȼ�����
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir); // ����н�
        if (angle > viewAngle / 2) return false; // ����׶�ǣ�����false
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, dir, out hit, detectionRange))
        {
            return hit.transform == playerTarget; // ���ڵ����������
        }
        return false;
    }

    void SwitchState(AIState newState)
    {
        currentState = newState;
        if (newState == AIState.Patrol) navAgent.SetDestination(waypoints[waypointIndex].position);
        // �ɼ���Ƶ��if (newState == AIState.Alert) GetComponent<AudioSource>().Play();
    }

    void OnDrawGizmos()
    {
        // ԭ��Ȧ����
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // �¼ӣ�׶�α߽�
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * detectionRange;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + leftBoundary);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + rightBoundary);
    }
}