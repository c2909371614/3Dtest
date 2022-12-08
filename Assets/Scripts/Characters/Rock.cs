using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitPlayer, HitEnemy, HitNothing}
    private Rigidbody rb;
    public RockStates rockStates;
    [Header("Basic Setting")]
    public float force;
    public Vector3 direction;
    public GameObject target;
    public int damage;
    public GameObject breakEffect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;//石头刚生成的速度为零，防止切换切换过早无法造成伤害
        FlyToTarget();
        rockStates = RockStates.HitPlayer;
    }
    private void FixedUpdate()//物理判断
    {
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }
    public void FlyToTarget()
    {
        if (target == null)//目标刚好丢死也能扔出石头
            target = FindObjectOfType<PlayerControl>().gameObject;
               
        direction = (target.transform.position - transform.position + transform.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);//给一个瞬间的力
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;//击退
                    collision.gameObject.GetComponent<Animator>().SetTrigger("dizzy");//眩晕
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());//伤害
                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Enemy_Golem>())//判断是否为石头人
                {
                    var collisionStats = collision.gameObject.GetComponent<CharacterStats>();
                    collisionStats.TakeDamage(damage, collisionStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);//生成碎石特效
                    Destroy(gameObject);
                }
                break;
        }
    }


}
