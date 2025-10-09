using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] waypoints; // 拖入空GameObject数组（关卡点2,3,4）
    public float detectionRange = 10f; // 检测范围
    public float alertDuration = 5f; // 警戒时间
    public Transform playerTarget; // 拖入玩家对象
    public float viewAngle = 90f; // 视野锥角度

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
                if (PlayerInLineOfSight()) SwitchState(AIState.Alert); // 修改这里：用完整FOV检查触发Alert
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
                if (!PlayerInLineOfSight()) SwitchState(AIState.Alert); // Chase回Alert也用FOV，避免丢失后立即回Patrol
                break;
        }
    }

    bool PlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, playerTarget.position) <= detectionRange;
    }

    bool PlayerInLineOfSight()
    {
        if (!PlayerInDetectionRange()) return false; // 先检查距离
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir); // 计算夹角
        if (angle > viewAngle / 2) return false; // 超出锥角，返回false
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, dir, out hit, detectionRange))
        {
            return hit.transform == playerTarget; // 无遮挡且命中玩家
        }
        return false;
    }

    void SwitchState(AIState newState)
    {
        currentState = newState;
        if (newState == AIState.Patrol) navAgent.SetDestination(waypoints[waypointIndex].position);
        // 可加音频：if (newState == AIState.Alert) GetComponent<AudioSource>().Play();
    }

    void OnDrawGizmos()
    {
        // 原红圈保持
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // 新加：锥形边界
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * detectionRange;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + leftBoundary);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + rightBoundary);
    }
}