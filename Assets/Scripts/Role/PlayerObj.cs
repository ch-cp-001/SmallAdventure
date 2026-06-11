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
    private float atkContinousTimer;

    // 水平和垂直方向速度输入值
    private float hSpeed;
    private float vSpeed;

    // 1表示向右，-1表示向左
    private int faceDir = 1;

    // 剑士攻击检测中心偏移值和半径
    public float atkOffsetCenter;
    public float atkOffsetRadius;
    public float atkRepalVelocity;

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
            return;
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
            return;
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

        // 只有在非防御状态下才允许移动
        if (state != RoleState.Guard)
        {
            hSpeed = Input.GetAxis("Horizontal");
            vSpeed = Input.GetAxis("Vertical");
            if (hSpeed != 0 || vSpeed != 0)
            {
                ChangeState(RoleState.Run);
            }
            else
            {
                ChangeState(RoleState.Idle);
            }
        }
    }

    private void FixedUpdate()
    {
        if(state == RoleState.Run)
        {
            rb.velocity = new Vector2(hSpeed, vSpeed) * speed;
        }
        if(state == RoleState.Idle || state == RoleState.Guard)
            rb.velocity = Vector2.zero;

    }

    public void Atk(int atkState)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x + faceDir * atkOffsetCenter, transform.position.y), atkOffsetRadius, 1<<LayerMask.NameToLayer("Monster"));
        foreach (Collider2D collider in colliders)
        {
            float angle = Vector2.Angle(transform.right*faceDir,collider.transform.position-transform.position);           
            if(collider.transform.position.y > transform.position.y && angle < 90 || 
                collider.transform.position.y < transform.position.y && angle <150)
            {
                EnemyObj obj = collider.gameObject.GetComponent<EnemyObj>();
                if (obj != null)
                {
                    obj.Damage(atk - obj.def);
                    if(atkState == 1)
                    {

                    }
                    else if(atkState == 2)
                    {
                        // 击退
                        obj.GetComponent<Rigidbody2D>().velocity = (obj.transform.position - transform.position).normalized * atkRepalVelocity;
                    }
                }
            }
        }
    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector2(transform.position.x+faceDir*atkOffsetCenter,transform.position.y),atkOffsetRadius);
        Gizmos.DrawRay(transform.position,new Vector3(-0.866f,-0.5f,0));
    }
}
