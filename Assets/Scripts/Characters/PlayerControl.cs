using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerControl : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private GameObject attackTarget;
    private float lastAttackTime;
    private CharacterStats characterStats;
    private bool isDeath;
    private float stopDistance;
    public float speed;
    public Rigidbody rb;
    //public GameObject playerGameObject;
    void Awake()
    {
        Debug.Log("Player");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
        //playerGameObject.SetActive(false);
    }
    private void OnEnable()
    {
        Debug.Log("PlayerOnEnable!!!!!");
        //if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }
    void Start()
    {
        Debug.Log("playerStart");
        SaveManager.Instance.LoadPlayerData();//加载玩家数据
    }
    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
        Debug.Log("PlayerDisable");
    }
    void Update()
    {
        isDeath = characterStats.CurrentHealth <= 0;//判断死亡
        if (isDeath)
            GameManager.Instance.NotifyObserver();//广播死亡消息
        //MoveMent();//wasd控制移动
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }
    private void SwitchAnimation()
    {
        anim.SetFloat("speed", agent.velocity.sqrMagnitude);
        anim.SetBool("death", isDeath);
    }

    //wasd控制移动
    public void MoveMent()
    {
        Debug.Log(Input.GetAxis("RightLeftMove"));
        float rightLeftMove = Input.GetAxis("RightLeftMove");
        float faceRightLeft = Input.GetAxisRaw("RightLeftMove");
        float frontBackMove = Input.GetAxis("FrontBackMove");
        float faceUpDown = Input.GetAxisRaw("FrontBackMove");
        Debug.Log(rightLeftMove);
        Debug.Log(frontBackMove);
        float rotationY = transform.localRotation.y;
        if (rightLeftMove != 0)
        {
            agent.enabled = false;

            rb.velocity = new Vector3(rightLeftMove * -speed, rb.velocity.y, rb.velocity.z);
            transform.localRotation = Quaternion.AngleAxis(rotationY + rightLeftMove * -90f, transform.up);
            anim.SetFloat("speed", Mathf.Abs(faceRightLeft));
        }
        if (frontBackMove != 0)
        {
            agent.enabled = false;
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, frontBackMove * speed);
            //transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, frontBackMove);
            //if (frontBackMove < 0)
            //    transform.localRotation = Quaternion.AngleAxis(rotationY + frontBackMove * 180f, transform.up);
            //else
            //    transform.localRotation = Quaternion.AngleAxis(rotationY, transform.up);
            anim.SetFloat("speed", Mathf.Abs(faceUpDown));
        }
    }

    //移动事件
    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDeath) return;//判断死亡
        agent.stoppingDistance = stopDistance;//移动时将停留距离改回 
        agent.isStopped = false;
        agent.destination = target;
    }
    //攻击事件
    private void EventAttack(GameObject target)
    {
        if (isDeath) return;
        if(target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());//协程
        }
    }
    //触发攻击
    IEnumerator MoveToAttackTarget()
    {//距离不够则移动到目标位置并攻击，距离够则直接攻击
        if(attackTarget != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.stoppingDistance = characterStats.attackData.attackRange;//把停留距离改成攻击距离
            transform.LookAt(attackTarget.transform);
            //TODO:修改攻击范围
            while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
            {
                agent.destination = attackTarget.transform.position;
                yield return null;
            }
            agent.isStopped = true;
            //开始攻击
            if (lastAttackTime < 0)
            {
                anim.SetTrigger("attack");
                anim.SetBool("isCritical", characterStats.isCritical);
                lastAttackTime = characterStats.attackData.coolDown;
            }
        }
        
    }
    //Animation event
    //攻击效果
    void Hit()
    {
        SoundManager.Instance.HitAudio();//播放打击声音
        if (attackTarget == null) return;
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>())
            {//击打石头
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
        
    }


}
