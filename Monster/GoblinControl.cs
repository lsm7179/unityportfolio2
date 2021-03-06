﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class GoblinControl : MonoBehaviour
{
    public enum GoblinState { None,Idle,Patrol,Wait, MoveToTarget,Attack,Damage,Die}


    [Header("기본속성")]
    public GoblinState goblinState = GoblinState.None;
    public float MoveSpeed = 1.0f;
    public GameObject TargetPlayer = null;
    public Transform TargetTransform = null;
    public Vector3 TargetPosition = Vector3.zero;

    private Animation myAnimation = null;
    private Transform myTransform = null;

    [Header("애니메이션 클립")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip MoveAnimClip = null;
    public AnimationClip AttackAnimClip = null;
    public AnimationClip DamageAnimClip = null;
    public AnimationClip DieAnimClip = null;

    [Header("전투 속성")]
    public int HP = 100;
    public float AttackRange = 1.5f;
    public GameObject DamageEffact = null;
    public GameObject DieEffect = null;
    private Tweener effectTweener = null;
    private SkinnedMeshRenderer skinMeshRenderer = null;


    void Start()
    {
        goblinState = GoblinState.Idle;
        //캐싱
        myAnimation = GetComponent<Animation>();
        myTransform = GetComponent<Transform>();
        //애니메이션 클립들 기본 세팅
        myAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[MoveAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[AttackAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].layer = 10;
        myAnimation[DieAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].layer = 10;

        //애니메이션 이벤트 추가
        AddAnimationEvent(AttackAnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(DamageAnimClip, "OnDamageAnimFinished");
        AddAnimationEvent(DieAnimClip, "OnDieAnimFinished");
        //스킨 메쉬를 캐싱.
        skinMeshRenderer = myTransform.Find("body").GetComponent<SkinnedMeshRenderer>();
    }
    /// <summary>
    /// 고블린의 상태에 따라 동작을 제어하는 함수
    /// </summary>
    void CheckState()
    {
        switch (goblinState)
        {
            case GoblinState.Idle:
                IdleUpdate();//idle -> MoveTotarget, Patrol
                break;
            case GoblinState.MoveToTarget:
            case GoblinState.Patrol:
                MoveUpdate();//move -> wait, attack
                break;
            case GoblinState.Attack://attack -> die, moveToTarget
                AttackUpdate();
                break;
        }
    }
    /// <summary>
    /// 대기 상태일때의 동작
    /// </summary>
    void IdleUpdate()
    {
        //만약 타겟 플레이어가 없다면, 임의의 지점을 랜덤하게 선택해서 레이캐스트를 이용하여
        //임의의 지점의 높이값까지 구해서 그 임의의 지점으로 이동시켜주도록 합니다.
        if (TargetPlayer == null)
        {
            TargetPosition = new Vector3(myTransform.position.x + Random.Range(-10.0f, 10.0f), myTransform.position.y + 1000.0f//y값이 엄청 높은 상태
                , myTransform.position.z + Random.Range(-10.0f, 10.0f));
            Ray ray = new Ray(TargetPosition, Vector3.down);
            RaycastHit raycastHit = new RaycastHit();
            if(Physics.Raycast(ray,out raycastHit, Mathf.Infinity))
            {
                //임의의 위치의 높이값
                TargetPosition.y = raycastHit.point.y;
            }
            //상태를 정찰 상태로 변경.
            goblinState = GoblinState.Patrol;
        }
        else
        {
            //타겟이 있다면 타겟을 향해 이동합니다.
            goblinState = GoblinState.MoveToTarget;
        }
    }
    //이동 상태에서의 동작. Patrol, MoveToTarget
    void MoveUpdate()
    {
        //두 벡터의 차
        Vector3 diff = Vector3.zero;
        Vector3 lookAtPosition = Vector3.zero;

        switch (goblinState)
        {
            case GoblinState.Patrol:
                if(TargetPosition != Vector3.zero)
                {
                    diff = TargetPosition - myTransform.position;
                    //목표지점까지 거의 왔으면.
                    if(diff.magnitude < AttackRange)
                    {
                        StartCoroutine(WaitUpdate());
                        return;
                    }

                    lookAtPosition = new Vector3(TargetPosition.x, myTransform.position.y, TargetPosition.z);
                }
                break;
            case GoblinState.MoveToTarget:
                if(TargetPosition != null)
                {
                    diff = TargetPlayer.transform.position - myTransform.position;

                    if (diff.magnitude < AttackRange)
                    {
                        goblinState = GoblinState.Attack;
                       return;
                    }
                    lookAtPosition = new Vector3(TargetPlayer.transform.position.x, myTransform.position.y, TargetPlayer.transform.position.z);
                }
                break;
        }

        Vector3 direction = diff.normalized; //방향만 얻는다.
        direction = new Vector3(direction.x, 0.0f, direction.z);
        Vector3 moveAmount = direction * MoveSpeed * Time.deltaTime;

        //이동
        myTransform.Translate(moveAmount, Space.World);//관련된 축을 월드축방향으로 이동시킨다.

        //바라보게 하기
        myTransform.LookAt(lookAtPosition);
        
    }

    /// <summary>
    /// 대기하는 동작.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitUpdate()
    {
        goblinState = GoblinState.Wait;
        float waitTime = Random.Range(1.0f, 3.0f);
        yield return new WaitForSeconds(waitTime);
        goblinState = GoblinState.Idle;
    }
   
    /// <summary>
    /// 인지범위안에 다른 트리거나 플레이어가 들어왔다면 호출됩니다.
    /// </summary>
    /// <param name="target"></param>
    void OnSetTarget(GameObject target)
    {
        TargetPlayer = target;
        TargetTransform = TargetPlayer.transform;
        //타겟을 향해 이동하는 상태로 전환
        goblinState = GoblinState.MoveToTarget;
    }

    /// <summary>
    /// 공격 상태일때의 동작
    /// </summary>
    void AttackUpdate()
    {
        //이걸 사용하면 무겁다. 해서 diff 를 사용해서 하는데 공부용으로 쓴다.
        float distance = Vector3.Distance(TargetTransform.position, myTransform.position);
        if (distance > AttackRange + 0.5f)
        {
            //타켓과의 거리가 걸어지면 타겟으로 이동합니다.
            goblinState = GoblinState.MoveToTarget;
        }

    }

    /// <summary>
    /// 애니메이션을 재생시켜주는 함수.
    /// </summary>
    void AnimationControl()
    {
        switch (goblinState)
        {
            //대기하거나 기다릴때
            case GoblinState.Wait:
            case GoblinState.Idle:
                myAnimation.CrossFade(IdleAnimClip.name);
                break;
            // 이동중일때
            case GoblinState.Patrol:
            case GoblinState.MoveToTarget:
                myAnimation.CrossFade(MoveAnimClip.name);
                break;
            //공격할때
            case GoblinState.Attack:
                myAnimation.CrossFade(AttackAnimClip.name);
                break;
            //죽었을때
            case GoblinState.Die:
                myAnimation.CrossFade(DieAnimClip.name);
                break;
        }
    }

    /// <summary>
    /// 피격되었는지 확인 하는 함수
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //플레이어의 공격에 맞았다면
        if (other.gameObject.CompareTag("PlayerAttack"))
        {
            HP -= 10;
            if (HP > 0)
            {
                //피격 이펙트 생성.
                Instantiate(DamageEffact, other.transform.position, Quaternion.identity);
                myAnimation.CrossFade(DamageAnimClip.name);
            }
            else
            {
                goblinState = GoblinState.Die;
            }
            //myAnimation.CrossFade()
        }
    }

    void DamageTweenEffect()
    {
        //트윈이 재생중이면 중복 트위닝 세팅하지 않습니다.
        if (effectTweener != null && !effectTweener.isComplete)
        {
            return;
        }
        Color colorTo = Color.red;
        effectTweener = HOTween.To(skinMeshRenderer, 0.2f, new TweenParms().Prop("color", colorTo).Loops(1, LoopType.Yoyo).OnStepComplete(DamageTweenFinished));
    }

    void DamageTweenFinished()
    {
        skinMeshRenderer.material.color = Color.white;
    }

    void Update()
    {
        CheckState();
        AnimationControl();
    }

    //애니메이션 재생이 끝났을때 호출 될 이벤트 함수들
    void OnAttackAnimFinished()
    {
        Debug.Log("Attack Animation finished");
    }

    void OnDamageAnimFinished()
    {
        Debug.Log("Damage Animation finished");
    }

    void OnDieAnimFinished()
    {
        
        Debug.Log("Die Animation finished");
        //죽음 이펙트 생성
        Instantiate(DieEffect, myTransform.position, Quaternion.identity);
        //몬스터 오브젝트 삭제
        Destroy(gameObject);
    }

    /// <summary>
    /// 애니메이션 이벤트를 추가해주는 함수
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="funcName"></param>
    void AddAnimationEvent(AnimationClip clip, string funcName)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = funcName;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }
}
