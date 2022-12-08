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
        rb.velocity = Vector3.one;//ʯͷ�����ɵ��ٶ�Ϊ�㣬��ֹ�л��л������޷�����˺�
        FlyToTarget();
        rockStates = RockStates.HitPlayer;
    }
    private void FixedUpdate()//�����ж�
    {
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }
    public void FlyToTarget()
    {
        if (target == null)//Ŀ��պö���Ҳ���ӳ�ʯͷ
            target = FindObjectOfType<PlayerControl>().gameObject;
               
        direction = (target.transform.position - transform.position + transform.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);//��һ��˲�����
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;//����
                    collision.gameObject.GetComponent<Animator>().SetTrigger("dizzy");//ѣ��
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());//�˺�
                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Enemy_Golem>())//�ж��Ƿ�Ϊʯͷ��
                {
                    var collisionStats = collision.gameObject.GetComponent<CharacterStats>();
                    collisionStats.TakeDamage(damage, collisionStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);//������ʯ��Ч
                    Destroy(gameObject);
                }
                break;
        }
    }


}
