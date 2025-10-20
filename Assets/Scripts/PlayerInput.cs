using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour {

    PlayerCharacter player;
    LineRenderer throwLine;

    [HideInInspector]
    public Vector3 curGroundPoint;

    public Image imageTrap;

    void Start()
    {
        player = GetComponent<PlayerCharacter>();
        throwLine = GetComponentInChildren<LineRenderer>();
    }

    void Update() {
        Vector3 input;
        input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (input.magnitude > 1.0f)
        {
            input = input.normalized;
        }

        // 设置当前输入向量
        player.curInput = input;

        bool fire = false;
        if (TouchGroundPos())       // 判断时会修改curGroundPoint
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                fire = true;
            }
        }

        player.UpdateMove();
        player.UpdateAction(curGroundPoint, fire);
        player.UpdateAnim(curGroundPoint);

        if (player.curItem == "cigar")
        {
            DrawThrowLine(curGroundPoint);
        }
        else
        {
            HideThrowLine();
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (player.settingTrapStart + player.settingTrapInterval >= Time.time)
        {
            float d = (player.settingTrapStart + player.settingTrapInterval) - Time.time;
            imageTrap.fillAmount = 1 - d / player.settingTrapInterval;
        }
        else
        {
            imageTrap.fillAmount = 1;
        }
    }

    bool TouchGroundPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (!Physics.Raycast(ray, out hitInfo, 1000, LayerMask.GetMask("Ground")))
        {
            return false;
        }
        curGroundPoint = hitInfo.point;
        return true;
    }

    void HideThrowLine()
    {
        throwLine.positionCount = 0;
    }

    void DrawThrowLine(Vector3 _target)
    {
        Vector3 target = transform.InverseTransformPoint(_target);
        float d = target.magnitude;
        float h = d/3;

        int T = 20;      // 抛物线分成T段
        float T_half = T / 2.0f;
        float a = 2 * h / (T_half * T_half);
        float vx = target.x / T;
        float vz = target.z / T;

        List<Vector3> points = new List<Vector3>();
        points.Add(Vector3.zero);

        Vector3 p = Vector3.zero;
        float vy = T_half * a;
        for (int i = 0; i < T; i++)
        {
            p.x += vx;
            p.z += vz;
            p.y += vy - a /2.0f;
            vy -= a;
            points.Add(p);
        }

        throwLine.positionCount = points.Count;
        throwLine.SetPositions(points.ToArray());
    }

    public void OnSelectItem(string item)
    {
        if (player.dead) return;
        player.curItem = item;

        switch (item)
        {
            case "handgun":     // 手枪
                break;
            case "cigar":       // 香烟
                break;
            case "trap":        // 陷阱
                {
                    player.BeginTrap();
                }
                break;
            case "knife":
                break;
        }
    }

}
