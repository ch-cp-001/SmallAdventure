using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObj : RoleObj
{
    // 怪物巡逻点
    public List<Transform> patrolPosList;

    // 怪物巡逻的间隔时间（巡逻中间休息的时间）
    public float patrolInterval;
    // 休息计时器
    private float patrolIntervalTimer;

    // 当前的目标点位
    private Vector3 targetPos;

    private int faceDir=1;
    void Update()
    {
        if(patrolIntervalTimer > 0)
            patrolIntervalTimer -= Time.deltaTime;
        

        // 设置移动目标位置   
        if (targetPos == Vector3.zero)
        {
            // 未设置巡逻点时在出生点周围随机移动
            if (patrolPosList.Count == 0)
            {
                // 没有目标位置则随机选择目标位置移动                
                float ranX = Random.Range(-5, 5);
                float ranY = Random.Range(-5, 5);
                targetPos = transform.position + new Vector3(ranX, ranY, 0);
            }
            // 在一点处站岗保持Idle状态直到发现玩家
            else if (patrolPosList.Count == 1)
            {
                targetPos = patrolPosList[0].position;
            }
            // 大于等于2时在设置的巡逻点之间巡逻
            else
            {
                int ranIndex = Random.Range(0, patrolPosList.Count);
                targetPos = patrolPosList[ranIndex].position;
            }
        }
        
        if(Vector3.Distance(transform.position,targetPos) <= 0.1)
        {
            rb.velocity = Vector2.zero;
            targetPos = Vector3.zero;
            patrolIntervalTimer = patrolInterval;
            ChangeState(RoleState.Idle);
        }

        // 检测角色朝向
        if (rb.velocity.x > 0 && faceDir == -1)
        {
            sr.flipX = false;
            faceDir = 1;
        }
        else if (rb.velocity.x < 0 && faceDir == 1)
        {
            sr.flipX = true;
            faceDir = -1;
        }
    }

    private void FixedUpdate()
    {
        // 向目标位置移动
        if(targetPos != Vector3.zero && patrolIntervalTimer <= 0)
        {
            ChangeState(RoleState.Run);
            rb.velocity = (targetPos - transform.position).normalized * speed;
        }
    }


      
}
