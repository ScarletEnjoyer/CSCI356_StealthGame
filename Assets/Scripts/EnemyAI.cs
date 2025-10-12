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
    public float attackRange = 2f; // ������Χ�������
    public float attackCooldown = 1f; // �����ȴʱ��

    private NavMeshAgent navAgent;
    private enum AIState { Patrol, Alert, Chase };
    private AIState currentState = AIState.Patrol;
    private int waypointIndex = 0;
    private float alertTimer = 0f;
    private Animator animator; // Animator ����
    private float attackTimer = 0f; // �����ʱ��

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // ��ȡ Animator
        if (waypoints.Length > 0) navAgent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        attackTimer += Time.deltaTime; // ���������ʱ��

        switch (currentState)
        {
            case AIState.Patrol:
                if (navAgent.remainingDistance < 0.5f && !navAgent.pathPending)
                {
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    navAgent.SetDestination(waypoints[waypointIndex].position);
                }
                animator.SetFloat("Speed", navAgent.velocity.magnitude); // ͬ��Ѳ���ٶ�
                if (PlayerInLineOfSight()) SwitchState(AIState.Alert);
                break;
            case AIState.Alert:
                navAgent.isStopped = true;
                alertTimer += Time.deltaTime;
                animator.SetFloat("Speed", 0f); // ����ʱֹͣ�ƶ�����
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
                animator.SetFloat("Speed", navAgent.velocity.magnitude); // ͬ��׷���ٶ�
                // ����������߼���ʹ�� Trigger �������޸�Ϊ SetTrigger������ Bool �ֶ����ã�
                if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange && attackTimer >= attackCooldown)
                {
                    animator.SetTrigger("Shoot"); // �������������һ�����¼���
                    attackTimer = 0f; // ������ȴ
                    // ��������ʵ�ʹ����߼���������ӵ����˺����
                }
                if (!PlayerInLineOfSight()) SwitchState(AIState.Alert);
                break;
        }
    }

    bool PlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, playerTarget.position) <= detectionRange;
    }

    bool PlayerInLineOfSight()
    {
        if (!PlayerInDetectionRange()) return false;
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2) return false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, dir, out hit, detectionRange))
        {
            return hit.transform == playerTarget;
        }
        return false;
    }

    void SwitchState(AIState newState)
    {
        currentState = newState;
        if (newState == AIState.Patrol)
        {
            navAgent.SetDestination(waypoints[waypointIndex].position);
            // �������� Shoot Trigger����Ϊ���Զ�����
        }
        else if (newState == AIState.Alert)
        {
            // �������� Shoot Trigger
        }
        else if (newState == AIState.Chase)
        {
            // Chase ʱ��������������� Update ������
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * detectionRange;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + leftBoundary);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + rightBoundary);
    }
}