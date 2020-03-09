using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

//모바일에 최적화 된 스크립트 소스 작성
public class SkeleteonCtrl : MonoBehaviour {
    /*스켈레톤 컨트롤
      자신과 플레이어의 거리를 재서 추적과 공격을 한다.*/
    [SerializeField]
    private Transform SkeletonTr;
    [SerializeField]
    private Transform PlayerTr;
    public float MoveSpeed = 3.0f;
    public float TraceDist = 20f;
    private Animator Ani;
    [SerializeField]
    private Image hpBar;
    [SerializeField]
    private Canvas thisCanvas;
    private int HpInit = 100;
    private int Hp = 100;
    
    private bool isDie =false;
    public enum SkelState {idle=0,trace,attack,die};
    public static SkelState thisState = SkelState.idle;
    private GameObject hitEffect;
    public GameObject MpSphere = null;
    public GameObject HpSphere = null;
    private GameObject DestroyEffect = null;
    public Collider monsterSword = null;
    [SerializeField]
    private NavMeshAgent Navi;
    public float attackdist = 3.0f;
    public float tracedist = 10f;
    void Awake () {
        Navi = GetComponent<NavMeshAgent>();
        SkeletonTr = GetComponent<Transform>();
        PlayerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        Ani = GetComponent<Animator>();
        hpBar = GetComponentInChildren<Image>();
        thisCanvas = GetComponentInChildren<Canvas>();
        hitEffect = Resources.Load<GameObject>("Effect/HitParticle");
        DestroyEffect = Resources.Load<GameObject>("Effect/DestoryParticle");
        MpSphere = Resources.Load<GameObject>("Effect/MpSphere");
        HpSphere = Resources.Load<GameObject>("Effect/HpSphere");
        monsterSword.enabled = false;
        Navi.destination = PlayerTr.position;
    }


    //해당 오브젝트가 active 상태일때 메소드가 실행 된다.***
    void OnEnable()
    {
        StartCoroutine(Action());
        StartCoroutine(SkelStateCheck());
        //StartCoroutine(PlayerView());
    }

    //몬스터가 계속 바라보게 만들기
    IEnumerator PlayerView()
    {
        while (!isDie)
        {
            if (thisState == SkelState.attack)
            {
                SkeletonTr.rotation = Quaternion.Slerp(SkeletonTr.rotation, Quaternion.LookRotation(PlayerTr.position - SkeletonTr.position), Time.deltaTime * 8);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator SkelStateCheck()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.2f);
            float dist = Vector3.Distance(PlayerTr.position, SkeletonTr.position);
            if (dist <= attackdist)
            {
                thisState = SkelState.attack;
            }
            else if (dist <= tracedist&&dist>attackdist)
            {
                thisState = SkelState.trace;
            }
            else
            {
                thisState = SkelState.idle;
            }
        }
    }
    //몬스터 액션 확인
    IEnumerator Action()
    {
        while (!isDie)
        {
            
            switch (thisState) {
                case SkelState.trace:
                    Navi.isStopped = false;
                    Navi.destination = PlayerTr.position;
                    Ani.SetBool("IsTrace", true);
                    Ani.SetBool("IsAttack", false);
                    break;
                case SkelState.attack:
                    monsterSword.enabled = true;
                    Navi.isStopped = true;
                    Ani.SetBool("IsTrace", false);
                    Ani.SetBool("IsAttack", true);
                    yield return new WaitForSeconds(1.8f);
                    monsterSword.enabled = false;
                    break;
                case SkelState.idle:
                    Navi.isStopped = true;
                    Ani.SetBool("IsTrace", false);
                    break;
            }
            yield return null;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            Ani.SetTrigger("IsHit");
            Hit(other.transform.position);
            if (Hp > 0) MinusHp();
            other.enabled = false;
        }
    }

    void MinusHp()
    {
        Hp -= 35;
        hpBar.fillAmount = (float)Hp / (float)HpInit;
        
        if(Hp <= 0)//죽었을때
        {
            Die();
        }
    }

    void Hit(Vector3 hitPos)
    {
        GameObject hitEff = Instantiate(hitEffect, hitPos, Quaternion.identity);
        Destroy(hitEff, 0.5f);
    }
    void Die()//캔버스 , hp 초기화, 내비게이션 초기화
    {
        thisState = SkelState.die;
        isDie = true;
        Ani.SetBool("IsTrace", false);
        Ani.SetTrigger("IsDie");
        thisCanvas.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        Navi.isStopped = true;
        StopAllCoroutines();
        StartCoroutine(PushPool());

    }

    IEnumerator PushPool()
    {
        yield return new WaitForSeconds(2.0f);
        DestoryEffect();
        //mp, hp 회복구
        //MpSphere.SetActive(true);
        float mpx = transform.position.x + Random.Range(-2, 2);
        float mpz = transform.position.z + Random.Range(-2, 2);
        float hpx = transform.position.x + Random.Range(-2, 2);
        float hpz = transform.position.z + Random.Range(-2, 2);
        GameObject MpSphere_ = (GameObject)Instantiate(MpSphere, new Vector3(mpx, MpSphere.transform.position.y, mpz), Quaternion.identity);
        MpSphere_.name = "MpSphere";
        GameObject HpSphere_ = (GameObject)Instantiate(HpSphere, new Vector3(hpx, HpSphere.transform.position.y, hpz), Quaternion.identity);
        HpSphere_.name = "HpSphere";
        yield return new WaitForSeconds(1.0f);
        isDie = false;
        thisState = SkelState.idle;
        thisCanvas.enabled = true;
        hpBar.fillAmount = 1.0f;
        Hp = 100;
        GetComponent<CapsuleCollider>().enabled = true;
        gameObject.SetActive(false);
    }

    void DestoryEffect()
    {
        GameObject DestoryEffect_ = Instantiate(DestroyEffect, this.transform.position, Quaternion.identity);
        Destroy(DestoryEffect_, 0.8f);
    }

    /// <summary>
    /// 추적하는 함수
    /// </summary>
    /*void Trace()
    {
        if (thisState != SkelState.trace)
        {
            return;
        }
        SkeletonTr.rotation = Quaternion.Slerp(SkeletonTr.rotation, Quaternion.LookRotation(PlayerTr.position - SkeletonTr.position), Time.deltaTime * 8);
        SkeletonTr.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }*/
}
