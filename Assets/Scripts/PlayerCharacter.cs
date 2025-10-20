using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    // 游戏中的角色属性
    public float moveSpeed;
    public float hp = 9999;
    [HideInInspector]
    public bool dead = false;

    [HideInInspector]
    public Vector3 curInput;            // 当前输入方向

    // 引用其它组件
    CharacterController cc;
    PlayerInput playerInput;
    Animator anim;

    // 当前道具
    public string curItem;

    // 武器相关
    public Transform weaponSlot;          // 武器槽位
    public Transform gunfirePos;          // 定位枪口闪光
    public ParticleSystem prefabGunFire;  // 枪口火光预制体
    public Transform prefabBullet;

    [HideInInspector]
    public float nextShootTime;         // 下次可以开火的时间
    public float shootInterval = 0.3f;  // 射击间隔

    // 音效相关
    AudioSource audioSource;
    AudioSource audioSourceFoot;
    AudioClip handgunSound;
    AudioClip footstepSound;

    // 陷阱相关逻辑
    [HideInInspector]
    public float settingTrapTime;               // 陷阱设置完毕的时刻
    [HideInInspector]
    public float settingTrapStart = -100;       // 开始设置陷阱的时刻
    public float settingTrapInterval = 4.0f;    // 陷阱道具CD时间
    public Transform prefabTrap;                // 陷阱预制体

    // 投掷物相关
    [HideInInspector]
    public Vector3 throwTarget;                 // 投掷目标点
    public Throwing prefabCigar;                // 投掷物预制体

    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        anim.SetBool("Static_b", true);

        weaponSlot.gameObject.SetActive(false);

        var sources = GetComponents<AudioSource>();
        audioSource = sources[0];
        audioSourceFoot = sources[1];
        handgunSound = Resources.Load<AudioClip>("Sound/FIREARM_Handgun_H_P30L_9mm_Fire_RR2_stereo");
        footstepSound = Resources.Load<AudioClip>("Sound/脚步沙地");

        gunfirePos = weaponSlot.Find("gunfirePos");
    }

    bool IsBusy()
    {
        if (curItem == "trap" && settingTrapTime >= Time.time)
        {
            return true;
        }
        return false;
    }

    public void UpdateMove()
    {
        if (dead) return;
        float curSpeed = moveSpeed;
        switch (curItem)
        {
            case "none":
                break;
            case "handgun":     // 手枪
                {
                    curSpeed = moveSpeed / 2.0f;
                }
                break;
            case "cigar":       // 香烟
                {
                    curSpeed = moveSpeed / 2.0f;
                }
                break;
            case "trap":        // 陷阱
                break;
            case "charm":       // 魅惑
                curSpeed = 0;
                break;
            case "knife":
                curSpeed = moveSpeed / 2.0f;
                break;
        }


        // 重力下落
        Vector3 v = curInput;
        if (!cc.isGrounded)
        {
            v.y = -0.5f;
        }

        cc.Move(v * curSpeed * Time.deltaTime);
    }

    float footSoundTime = 0;
    void PlayFootstepSound()
    {
        if (footSoundTime < Time.time)
        {
            audioSourceFoot.clip = footstepSound;
            audioSourceFoot.Play();

            footSoundTime = Time.time + 0.34f;
        }
    }

    public void UpdateAnim(Vector3 curInputPos)
    {
        if (dead)
        {
            anim.SetFloat("Speed_f", 0);
            return;
        }
        anim.SetFloat("Speed_f", cc.velocity.magnitude / moveSpeed);
        if (cc.velocity.magnitude > moveSpeed / 1.9f)
        {
            PlayFootstepSound();
        }
        weaponSlot.gameObject.SetActive(false);

        if (curItem == "handgun")
        {
            anim.SetLayerWeight(anim.GetLayerIndex("Weapons"), 1);
            weaponSlot.gameObject.SetActive(true);
            weaponSlot.Find("Weapon_Pistol").gameObject.SetActive(true);
            weaponSlot.Find("Weapon_Knife").gameObject.SetActive(false);
            Vector3 v = new Vector3(curInputPos.x, transform.position.y, curInputPos.z);
            transform.LookAt(v);
        }
        else if (curItem == "knife")
        {
            anim.SetLayerWeight(anim.GetLayerIndex("Weapons"), 1);
            weaponSlot.gameObject.SetActive(true);
            weaponSlot.Find("Weapon_Pistol").gameObject.SetActive(false);
            weaponSlot.Find("Weapon_Knife").gameObject.SetActive(true);
            Vector3 v = new Vector3(curInputPos.x, transform.position.y, curInputPos.z);
            transform.LookAt(v);
        }
        else if (curItem == "trap")
        {
            anim.SetFloat("Speed_f", 0);
            anim.SetInteger("Animation_int", 9);
        }
        else if (curItem == "charm")
        {
            anim.SetFloat("Speed_f", 0);
            anim.SetInteger("Animation_int", 4);
            transform.Rotate(0, 1, 0);
        }
        else
        {
            anim.SetInteger("Animation_int", 0);
            anim.SetLayerWeight(anim.GetLayerIndex("Weapons"), 0);
            if (curInput.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(curInput);
            }
        }

    }

    public void UpdateAction(Vector3 curInputPos, bool fire)
    {
        if (dead) return;
        switch (curItem)
        {
            case "none":
                break;
            case "handgun":     // 手枪
                {
                    anim.SetInteger("WeaponType_int", 6);
                    if (fire)
                    {
                        Shoot(curInputPos);
                        anim.SetBool("Shoot_b", true);
                        //BigNoise();
                    }
                    else
                    {
                        anim.SetBool("Shoot_b", false);
                    }
                }
                break;
            case "cigar":       // 香烟
                {
                    throwTarget = curInputPos;
                    if (fire)
                    {
                        curItem = "none";
                        ThrowItem(prefabCigar);
                    }
                }
                break;
            case "trap":        // 陷阱
                if (Time.time > settingTrapTime)
                {
                    curItem = "none";
                    PlaceTrap();
                }
                break;
            case "knife":
                anim.SetInteger("WeaponType_int", 101);
                if (fire)
                {
                    Stab();
                    anim.SetBool("Shoot_b", true);
                }
                else
                {
                    anim.SetBool("Shoot_b", false);
                }
                break;
        }
    }

    public void BeginTrap()
    {
        if (settingTrapStart + settingTrapInterval > Time.time)
        {
            curItem = "none";
            return;
        }
        settingTrapTime = Time.time + 2;
    }

    void PlaceTrap()
    {
        Instantiate(prefabTrap, transform.position, Quaternion.identity);
        settingTrapStart = Time.time;
    }

    void Shoot(Vector3 target)
    {
        if (Time.time < nextShootTime)
        {
            return;
        }
        Vector3 dir = target - transform.position;
        dir.y = 0;
        dir = dir.normalized;

        Vector3 startPos = new Vector3(transform.position.x, transform.position.y + cc.height / 2, transform.position.z);
        startPos += dir * (2.5f * cc.radius);

        Transform bullet = Instantiate(prefabBullet, startPos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        b.Init("Enemy", dir, 0.2f);

        PlaySound(handgunSound);

        var particle = Instantiate(prefabGunFire, gunfirePos.position, Quaternion.identity);

        var light = gunfirePos.GetComponent<Light>();
        light.enabled = true;

        StartCoroutine(_flash(light));

        nextShootTime = Time.time + shootInterval;
    }

    void ThrowItem(Throwing item)
    {
        Vector3 startPos = new Vector3(transform.position.x, transform.position.y + cc.height / 2, transform.position.z);
        startPos += transform.forward * (2.0f * cc.radius);
        Throwing obj = Instantiate(item, startPos, Quaternion.identity);
        obj.throwTarget = throwTarget;
    }

    IEnumerator _flash(Light light)
    {
        if (light == null) { yield break; }
        light.enabled = true;
        yield return new WaitForSeconds(0.1f);
        light.enabled = false;
    }

    void BigNoise()
    {
        EnemyCharacter[] enemies = FindObjectsOfType<EnemyCharacter>();
        foreach (var enemy in enemies)
        {
            enemy.HearNoise(transform);
        }
    }

    void Stab()
    {
        float stabDist = 2f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, stabDist);

        // 对面前的敌人实施斩杀
        foreach (var c in colliders)
        {
            Vector3 to = c.gameObject.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, to) > 30)
            {
                continue;
            }
            if (c.gameObject.tag != "Enemy")
            {
                continue;
            }
            Ray ray = new Ray(transform.position, to);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo, stabDist))
            {
                continue;
            }
            if (hitInfo.collider != c)
            {
                continue;
            }

            c.GetComponent<EnemyCharacter>().BeHit(10000);
        }
    }

    void Die()
    {
        transform.Rotate(90, 0, 0);
        dead = true;

        GameMode.Instance.GameOver();
    }

    public void BeHit(float damage)
    {
        if (dead) return;
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    public void DisableSelf()
    {
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        GetComponentInChildren<Collider>().enabled = false;
        this.enabled = false;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
