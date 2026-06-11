using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoleState
{
    Idle,
    Run,
    Guard,
    Atk1,
    Atk2,
    Atk3,
}



public class RoleObj : SingletonBaseMono<RoleObj>
{
    [Header("Properties")]
    public int hp;
    public int mp;
    public int atk;
    public int def;
    public float speed;

    protected RoleState state=RoleState.Idle;
    protected Animator ani;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    private void Start()
    {
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ChangeState(RoleState.Idle);
        //Init();
    }

    //protected virtual void Init() { };

    public void ChangeState(RoleState newState)
    {
        ani.SetBool(state.ToString(),false);
        ani.SetBool(newState.ToString(),true);
        state = newState;
    }
}
