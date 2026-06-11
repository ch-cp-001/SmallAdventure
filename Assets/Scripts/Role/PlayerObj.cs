using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObj : RoleObj
{
    // 上次攻击状态
    private RoleState preAtkState;
    // 连续攻击的中断事件
    public float atkContinous;
    // 连续攻击的计时器
    public float atkContinousTimer;

    public float hSpeed;
    public float vSpeed;

    // 1表示向右，-1表示向左
    private int faceDir = 1;

    //protected override void Init()
    //{
        

    //}

    private void Update()
    {

        // 检测防御
        if (Input.GetMouseButton(1))
        {
            ChangeState(RoleState.Guard);
            preAtkState = RoleState.Guard;
        }
        if (Input.GetMouseButtonUp(1))
        {
            ChangeState(RoleState.Idle);
            preAtkState = RoleState.Idle;
        }

        // 检测攻击
        if (Input.GetMouseButtonDown(0))
        {
            if (preAtkState == RoleState.Atk1)
            {
                ChangeState(RoleState.Atk2);
                preAtkState = RoleState.Atk2;
            }
            else if (preAtkState == RoleState.Atk2)
            {
                ChangeState(RoleState.Atk3);
                preAtkState = RoleState.Atk3;
            }
            else
            {
                ChangeState(RoleState.Atk1);
                preAtkState = RoleState.Atk1;
            }
            atkContinousTimer = atkContinous;
        }
        // 计算攻击中断间隔时间
        if (atkContinousTimer > 0)
        {
            atkContinousTimer -= Time.deltaTime;
            if (atkContinousTimer <= 0)
            {
                preAtkState = RoleState.Idle;
            }
        }

        // 检测角色朝向
        if (hSpeed > 0 && faceDir == -1)
        {
            sr.flipX = false;
            faceDir = 1;
        }
        else if (hSpeed < 0 && faceDir == 1)
        {
            sr.flipX = true;
            faceDir = -1;
        }
    }

    private void FixedUpdate()
    {
        // 只有在非防御状态下才允许移动
        if (state != RoleState.Guard)
        {
            hSpeed = Input.GetAxis("Horizontal");
            vSpeed = Input.GetAxis("Vertical");
            if (hSpeed != 0 || vSpeed != 0)
            {
                ChangeState(RoleState.Run);
                rb.velocity = new Vector2(hSpeed, vSpeed) * speed;
            }
            else
            {
                ChangeState(RoleState.Idle);
                rb.velocity = Vector2.zero;
            }
        }
        
    }
}
