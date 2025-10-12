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
    public float attackRange = 2f; // 攻击范围（射击）
    public float attackCooldown = 1f; // 射击冷却时间

    private NavMeshAgent navAgent;
    private enum AIState { Patrol, Alert, Chase };
    private AIState currentState = AIState.Patrol;
    private int waypointIndex = 0;
    private float alertTimer = 0f;
    private Animator animator; // Animator 引用
    private float attackTimer = 0f; // 射击计时器

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // 获取 Animator
        if (waypoints.Length > 0) navAgent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        attackTimer += Time.deltaTime; // 更新射击计时器

        switch (currentState)
        {
            case AIState.Patrol:
                if (navAgent.remainingDistance < 0.5f && !navAgent.pathPending)
                {
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    navAgent.SetDestination(waypoints[waypointIndex].position);
                }
                animator.SetFloat("Speed", navAgent.velocity.magnitude); // 同步巡逻速度
                if (PlayerInLineOfSight()) SwitchState(AIState.Alert);
                break;
            case AIState.Alert:
                navAgent.isStopped = true;
                alertTimer += Time.deltaTime;
                animator.SetFloat("Speed", 0f); // 警戒时停止移动动画
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
                animator.SetFloat("Speed", navAgent.velocity.magnitude); // 同步追逐速度
                // 近距离射击逻辑：使用 Trigger 触发（修改为 SetTrigger，避免 Bool 手动重置）
                if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange && attackTimer >= attackCooldown)
                {
                    animator.SetTrigger("Shoot"); // 触发射击动画（一次性事件）
                    attackTimer = 0f; // 重置冷却
                    // 这里可添加实际攻击逻辑，如射击子弹或伤害玩家
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
            // 无需重置 Shoot Trigger，因为它自动重置
        }
        else if (newState == AIState.Alert)
        {
            // 无需重置 Shoot Trigger
        }
        else if (newState == AIState.Chase)
        {
            // Chase 时不立即射击，依赖 Update 检查距离
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