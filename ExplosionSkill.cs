using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ExplosionSkill : MonoBehaviour
{
    //충돌체 검출 반경
    public float radius = 5.0f;
    public float power = 300.0f;
    public float upwardsModifier = 5.0f;

    Rigidbody rigid = null;

    //카메라 쉐이크
    Transform camaraTr;
    Vector3 originPos;

    private void Start()
    {
        Vector3 explosionPos = transform.position;

        //explosionPos 위치의 radius 반경의 콜라이더를 얻어 올수 있다.
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach(Collider col in colliders)
        {
            if (!col.gameObject.CompareTag("Monster"))
            {
                continue;   //스킵
            }
            rigid = col.GetComponent<Rigidbody>();
            Vector3 monsterPosition = col.GetComponent<Transform>().position;
            if (rigid != null)                                                                                                          
            {
                rigid.AddExplosionForce(power, explosionPos, radius, upwardsModifier);
                col.gameObject.SendMessage("SkillHit");
            }
        }
        Destroy(this.gameObject, 1.2f);
        camaraTr = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        originPos = camaraTr.position;
        StartCoroutine(Shake(0.5f,0.2f));

    }
    private void OnDestroy()
    {
        if (rigid != null)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
       
    
    }

    public IEnumerator Shake(float _amount
    , float _duration)
    {
        float timer = 0;
        while (timer <= _duration)
        {
            camaraTr.localPosition = (Vector3)Random.insideUnitCircle * _amount + originPos;

            timer += Time.deltaTime;
            yield return null;
        }
        camaraTr.localPosition = originPos;

    }
}
