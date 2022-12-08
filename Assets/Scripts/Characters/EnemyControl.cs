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
    //��϶���
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
        //TODO:�����л���ĵ�
        GameManager.Instance.AddObserver(this);
    }
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}
    void OnDisable()
    {
        //ɾ��Ԥ���㲥����
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }
    private void Update()
    {
        if (!playerDeath)//playerû��
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
        //�붯����������
        anim.SetBool("walk", isWalk);
        anim.SetBool("chase", isChase);
        anim.SetBool("follow", isFollow);
        anim.SetBool("isCritical", characterStats.isCritical);
        anim.SetBool("death", isDeath);
    }
    void SwitchStates()
    {
        //�ж�״̬���л�״̬
        if (isDeath)
            enemyStates = EnemyStates.DEAD;
        //�������player���л���CHUASE
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            agent.speed = speed; 
            //Debug.Log("found player"); 
        }
        switch (enemyStates)
        {
            case EnemyStates.GUARD://����״̬
                isChase = false;//�ر�׷��
                if(transform.position != guardPos)
                {
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    isWalk = true;//����walk����
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;//����ֹͣ����walk����
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL: //Ѳ��
                isChase = false;
                agent.speed = speed * 0.5f;
                //�Ƿ񵽴�Ѳ�ߵ�
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
            case EnemyStates.CHASE://׷��
                isWalk = false;
                isChase = true;
                agent.speed = speed;

                if (!FoundPlayer())
                {
                    //����
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
                //�ڹ�����Χ���򹥻�
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //�����ж� Random.valueΪ���0-1����
                        characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
                        //ִ�й���
                        Attack();
                    }
                }
                else
                {
                    agent.isStopped = false;
                }
                break;
            case EnemyStates.DEAD://����
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;//��ֹʬ�嵲·
                Destroy(gameObject, 2f);//�ӳ�2������
                break;
        }
    }
    void Attack()
    {
        //��Ŀ�귢����Ӧ����
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
        //Ѱ��Ŀ��
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
        //�չ���������
        if(attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange; 
        return false;
    }
    bool TargetInSkillRange()
    {
        //���ܹ�������
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        return false;
    }
    void GetNewWayPoint()
    {
        //��ȡ���Ѳ�ߵ�λ
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        //TODO:���ܳ�������
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
        //wayPoint = randomPoint;
    }
    private void OnDrawGizmosSelected()
    {
        //��������ע�������
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }
    //Animation Hit
    void Hit()
    {
        //����˺�
        if(attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //֪ͨ
        //��ʤ����
        //ֹͣ�����ƶ�
        //ֹͣagent
        //Debug.Log("Slime Win");
        anim.SetBool("win", true); 
        isChase = false;
        isWalk = false;
        attackTarget = null;
        playerDeath = true;
    }
}
