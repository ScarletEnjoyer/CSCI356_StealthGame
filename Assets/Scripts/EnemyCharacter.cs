using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Patrol,
    Attack,
    Chase,
    Lookup,
    Die,
    Freeze,
}

public class EnemyCharacter : MonoBehaviour {

    Animator anim;
    NavMeshAgent agent;

    float walkSpeed = 2.5f;
    float runSpeed = 8f;

    public List<Transform> patrolPoints;
    int patrolIndex;

    Transform attackTarget;
    Vector3 LookupPos;

    float hp = 3;
    CapsuleCollider coll;

    public Transform prefabBullet;
    public Transform weaponSlot;
    public Transform bloodSplatter;

    float nextShootTime = 0;
    public float shootInterval = 0.1f;

    AudioSource audioSource;
    AudioClip submgunSound;
    AudioClip dieSound;

    Transform viewIndicator;
    MeshRenderer viewRenderer;
    MeshFilter viewFilter;

    AIState _state;
    AIState state
    {
        get
        {
            return _state;
        }
        set
        {
            //Debug.Log("state change to " + value.ToString());
            _state = value;
        }
    }
    public float maxScanDist = 16f;
    public float maxAttackDist = 13f;
    public float maxChaseDist = 16f;
    float maxHearDistance = 50f;

    float freezeTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Static_b", true);
        anim.SetInteger("WeaponType_int", 6);

        agent = GetComponent<NavMeshAgent>();
        if (agent.avoidancePriority == 0)
        {
            agent.avoidancePriority = Random.Range(30, 61);
        }
        patrolIndex = 0;
        agent.isStopped = true;

        state = AIState.Patrol;
        coll = GetComponent<CapsuleCollider>();

        audioSource = GetComponent<AudioSource>();
        submgunSound = Resources.Load<AudioClip>("Sound/FIREARM_Sub_Machine_Gun_Model_01b_Fire_Single_RR2_stereo");
        dieSound = Resources.Load<AudioClip>("Sound/男惨叫轻_003");

        viewIndicator = transform.Find("ViewIndicator");
        viewFilter = viewIndicator.GetComponent<MeshFilter>();
        viewRenderer = viewIndicator.GetComponent<MeshRenderer>();
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

    void UpdateAttack()
    {
        if (attackTarget == null)
        {
            state = AIState.Patrol;
            return;
        }

        Transform target = UpdateScan();
        if (target == null)
        {
            state = AIState.Chase;
            agent.isStopped = false;
            return;
        }

        attackTarget = target;

        if (Vector3.Distance(attackTarget.position, transform.position) > maxAttackDist)
        {
            agent.isStopped = false;
            agent.SetDestination(attackTarget.position);
            return;
        }

        agent.isStopped = true;
        if (attackTarget.tag == "Player")
        {
            Shoot(attackTarget.position);
        }
        else if (attackTarget.tag == "Cigar")
        {
            attackTarget.GetComponent<Throwing>().DelayDestroy();
        }
        else if (attackTarget.tag == "Corpse")
        {
            attackTarget.GetComponentInChildren<Corpse>().DelayDestroy();
            // 报警
            GameMode.Instance.Alarm();
        }
    }


    Transform UpdateScan()
    {
        List<Vector3> points = new List<Vector3>();

        System.Func<bool> _DrawRange = () =>
        {
            //for (int i=1; i<points.Count; i++)
            //{
            //    Debug.DrawLine(transform.position+points[0], transform.position + points[i]);
            //}
            List<int> tris = new List<int>();
            for (int i = 2; i < points.Count; i++)
            {
                tris.Add(0);
                tris.Add(i - 1);
                tris.Add(i);
            }

            Mesh mesh = new Mesh();

            mesh.vertices = points.ToArray();
            mesh.triangles = tris.ToArray();
            //mesh.uv = uvs.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            viewFilter.mesh = mesh;
            return true;
        };



        Vector3 offset = new Vector3(0, 1, 0);
        points.Add(offset);
        for (int d = -60; d < 60; d += 4)
        {
            Vector3 v = Quaternion.Euler(0, d, 0) * transform.forward;

            Ray ray = new Ray(transform.position + offset, v);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo, maxScanDist))
            {
                Vector3 localv = transform.InverseTransformVector(v);
                points.Add(offset + localv * maxScanDist);
                //Debug.DrawLine(transform.position, transform.position+v*maxScanDist, Color.red);
            }
            else
            {
                Vector3 local = transform.InverseTransformPoint(hitInfo.point);
                points.Add(offset + local);
                //Debug.DrawLine(transform.position, hitInfo.point, Color.red);
            }
        }

        _DrawRange();

        Collider[] colliders = Physics.OverlapSphere(transform.position, maxScanDist);
        //Debug.DrawLine(transform.position, transform.position + transform.forward * maxScanDist);

        List<Transform> targets = new List<Transform>();
        foreach (var c in colliders)
        {
            Vector3 to = c.gameObject.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, to) > 60)
            {
                continue;
            }
            Ray ray = new Ray(transform.position + offset, to);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo, maxScanDist))
            {
                continue;
            }
            if (hitInfo.collider != c)
            {
                continue;
            }
            Debug.DrawLine(transform.position + offset, hitInfo.point, Color.blue);

            if (c.gameObject.tag == "Player")
            {
                return c.transform;     // 最高优先级
            }
            if (c.gameObject.tag == "Cigar")
            {
                targets.Add(c.transform);
            }
            if (c.gameObject.tag == "Corpse")
            {
                targets.Add(c.transform);
            }
        }

        targets.Sort((Transform a, Transform b) => {
            Dictionary<string, int> priorities = new Dictionary<string, int> {
                { "Player", 10 }, {"Cigar", 1 }, {"Corpse", 3},
            };
            int na = 0, nb = 0;
            priorities.TryGetValue(a.tag, out na);
            priorities.TryGetValue(a.tag, out nb);
            return na.CompareTo(nb);
        });

        if (targets.Count > 0)
        {
            return targets[targets.Count - 1];
        }
        return null;
    }

    void UpdateChase()
    {
        if (attackTarget == null)
        {
            state = AIState.Patrol;
            return;
        }
        if (Vector3.Distance(attackTarget.position, transform.position) > maxChaseDist)
        {
            state = AIState.Patrol;
            agent.isStopped = true;
            attackTarget = null;
            return;
        }

        Transform target = UpdateScan();
        if (target == null)
        {
            return;
        }

        if (Vector3.Distance(target.position, transform.position) < maxAttackDist)
        {
            state = AIState.Attack;
            attackTarget = target;
            agent.isStopped = true;
            return;
        }

        if (Vector3.Distance(target.position, agent.destination) > 0.5f)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
    }

    void UpdateLookup()
    {
        if (Vector3.Distance(agent.destination, LookupPos) > 1)
        {
            agent.isStopped = false;
            agent.SetDestination(LookupPos);
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            state = AIState.Patrol;
            return;
        }

        Transform target = UpdateScan();
        if (target == null)
        {
            return;
        }

        state = AIState.Chase;
        attackTarget = target;
    }

    void UpdateAI()
    {
        switch(state)
        {
            case AIState.Patrol:
                {
                    agent.speed = walkSpeed;
                    UpdatePatrol();
                    Transform target = UpdateScan();
                    if (target != null)
                    {
                        state = AIState.Chase;
                        attackTarget = target;
                    }
                }
                break;
            case AIState.Lookup:
                {
                    agent.speed = runSpeed;
                    UpdateLookup();
                }
                break;
            case AIState.Chase:
                {
                    agent.speed = runSpeed;
                    UpdateChase();
                }
                break;
            case AIState.Attack:
                {
                    UpdateAttack();
                }
                break;

            case AIState.Die:
                break;

            case AIState.Freeze:
                if (Time.time > freezeTime)
                {
                    state = AIState.Patrol;
                }
                break;
        }
    }

    void Update()
    {
        UpdateAI();
        UpdateAnim();
    }

    void UpdateAnim()
    {
        if (state == AIState.Die)
        {
            anim.SetFloat("Speed_f", 0);
            return;
        }

        if (state == AIState.Chase || state == AIState.Attack)
        {
            anim.SetBool("Shoot_b", true);
            weaponSlot.localRotation = Quaternion.Euler(0, 0, 50);
        }
        else
        {
            anim.SetBool("Shoot_b", false);
            weaponSlot.localRotation = Quaternion.Euler(0, 0, 0);
        }

        float speed = agent.velocity.magnitude / runSpeed;
        anim.SetFloat("Speed_f", speed);
    }

    public void BeHit(float damage)
    {
        if (hp <= 0)
        {
            return;
        }
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
        Instantiate(bloodSplatter, transform.position, Quaternion.Euler(0, Random.Range(0, 359), 0));
    }

    public void OnTrap(float time)
    {
        state = AIState.Freeze;
        freezeTime = Time.time + time;
        agent.isStopped = true;
    }

    void Die()
    {
        state = AIState.Die;
        agent.isStopped = true;
        agent.enabled = false;
        transform.Rotate(90, 0, 0);

        audioSource.clip = dieSound;
        audioSource.Play();

        Destroy(viewFilter);
        Destroy(viewRenderer);

        transform.Find("Corpse").gameObject.SetActive(true);
    }

    public bool IsDead()
    {
        return state == AIState.Die;
    }

    void Shoot(Vector3 target)
    {
        if (Time.time < nextShootTime)
        {
            return;
        }
        
        target.y = transform.position.y;
        transform.LookAt(target);

        Vector3 dir = target - transform.position;
        dir.y = 0;
        dir = dir.normalized;

        Vector3 startPos = new Vector3(transform.position.x, transform.position.y + coll.height / 2, transform.position.z);
        startPos += dir * (2.5f * coll.radius);

        Transform bullet = Instantiate(prefabBullet, startPos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        b.Init("Player", dir);

        audioSource.clip = submgunSound;
        audioSource.Play();

        nextShootTime = Time.time + shootInterval;
    }

    public void HearNoise(Transform target)
    {
        if (Vector3.Distance(target.position, transform.position) > maxHearDistance)
        {
            return;
        }
        if (state != AIState.Patrol)
        {
            return;
        }
        LookupPos = target.position;
        state = AIState.Lookup;
    }
}
