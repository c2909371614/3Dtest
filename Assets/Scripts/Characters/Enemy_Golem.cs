using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Golem : EnemyControl
{
    [Header("Skill")]
    public float kickForce = 25;
    public GameObject rockPerfab;
    public Transform handPos;
    //Animation Event
    public void KickOff()
    {
        //����˺�
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            //ѣ��
            targetStats.GetComponent<Animator>().SetTrigger("dizzy");

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
    //Animation Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            //��ָ��λ������ʯͷ����ת�Ƕ�Ϊ��
            var rock = Instantiate(rockPerfab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
