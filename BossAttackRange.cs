using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRange : MonoBehaviour
{
    //콜라이더 범위안에 플레이어가 있다면 true를 보내자.
    //없다면 false를 보내자.

    public bool AttackRangeChk()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 3.0f);
        foreach (Collider col in colls)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
}
