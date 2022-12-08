using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyControl : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;
    private EnemyStates enemyStates;
    private Collider coll;
    [Header("Basic Settings")]
    public float sightRadius;
    protected GameObject attackTarget;
    public bool isGuard;
    private float speed;
    private Animator anim;
    private Vector3 guardPos;
    private Quaternion guardRotation;
    
    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private float lastAttackTime;
    //配合动画
    bool isWalk, isChase, isFollow, isDeath, playerDeath;

    public float lookAtTime;
    private float remainLookAtTime;
    protected CharacterStats characterStats;
    private void Awake()
    {
        Debug.Log("Enemy");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }
    private void Start()
    {

        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            //Debug.Log("Patrol");
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //TODO:场景切换后改掉
        GameManager.Instance.AddObserver(this);
    }
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}
    void OnDisable()
    {
        //删除预备广播对象
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }
    private void Update()
    {
        if (!playerDeath)//player没死
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        if (characterStats.CurrentHealth <= 0)
            isDeath = true;
    }
    void SwitchAnimation()
    {
        //与动画参数关联
        anim.SetBool("walk", isWalk);
        anim.SetBool("chase", isChase);
        anim.SetBool("follow", isFollow);
        anim.SetBool("isCritical", characterStats.isCritical);
        anim.SetBool("death", isDeath);
    }
    void SwitchStates()
    {
        //判断状态并切换状态
        if (isDeath)
            enemyStates = EnemyStates.DEAD;
        //如果发现player，切换到CHUASE
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            agent.speed = speed; 
            //Debug.Log("found player"); 
        }
        switch (enemyStates)
        {
            case EnemyStates.GUARD://守卫状态
                isChase = false;//关闭追赶
                if(transform.position != guardPos)
                {
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    isWalk = true;//播放walk动画
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;//到了停止播放walk动画
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL: //巡逻
                isChase = false;
                agent.speed = speed * 0.5f;
                //是否到达巡逻点
                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
                {
                    //Debug.Log("notWalk");
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    //Debug.Log("walk");
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE://追击
                isWalk = false;
                isChase = true;
                agent.speed = speed;

                if (!FoundPlayer())
                {
                    //拉脱
                    isFollow = false;
                    if(remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                    
                }
                else
                {
                    isFollow = true;
                    //Debug.Log("found player");
                    agent.destination = attackTarget.transform.position;  
                }
                //在攻击范围内则攻击
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //暴击判断 Random.value为随机0-1的数
                        characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                else
                {
                    agent.isStopped = false;
                }
                break;
            case EnemyStates.DEAD://死亡
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;//防止尸体挡路
                Destroy(gameObject, 2f);//延迟2秒销毁
                break;
        }
    }
    void Attack()
    {
        //对目标发动对应攻击
        transform.LookAt(attackTarget.transform);
         if(TargetInSkillRange())
         {
            anim.SetTrigger("skill");
         }else if (TargetInAttackRange())
         {
            anim.SetTrigger("attack");
         }
    }
    bool FoundPlayer()
    {
        //寻找目标
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true; 
            }
        }
        attackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        //普攻攻击距离
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange; 
        return false;
    }
    bool TargetInSkillRange()
    {
        //技能攻击距离
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        return false;
    }
    void GetNewWayPoint()
    {
        //获取随机巡逻点位
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        //TODO:可能出现问题
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
        //wayPoint = randomPoint;
    }
    private void OnDrawGizmosSelected()
    {
        //画线做标注方便调试
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }
    //Animation Hit
    void Hit()
    {
        //造成伤害
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //通知
        //获胜动画
        //停止所有移动
        //停止agent
        //Debug.Log("Slime Win");
        anim.SetBool("win", true); 
        isChase = false;
        isWalk = false;
        attackTarget = null;
        playerDeath = true;
    }
}
