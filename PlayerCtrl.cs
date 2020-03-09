using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Holoville.HOTween;



public class PlayerCtrl : SingletonMonobehavior<PlayerCtrl>
{
    [SerializeField]
    public enum PlayerState { Idle, Walk, Run, Attack, Skill,DashSkill,Die };
    [Header("플레이어 상태")]
    public PlayerState myState = PlayerState.Idle;
    public float hpcur = 0f;//현재체력  
    public float mpcur = 0f;//현재마력

    public enum FighterAttackState { Attack1, Attack2, Attack3, Attack4 };
    [Header("공격 상태")]
    public FighterAttackState AttackState = FighterAttackState.Attack1;
    public bool NextAttack = false;//다음 공격 활성화 여부를 확인하는 플래그
    public bool CannotMove = false;//이동 불가 플래그.

    [Header("전투 관련")]
    public TrailRenderer AttackTrailRenderer = null;
    public BoxCollider AttackBoxCollider = null;
    public GameObject SkillEffect = null;
    public GameObject DashEffect = null;
    public bool DashMoving = false;
    public float time=0.0f;
    private Tweener effectTweener = null;
    public Image hpImage = null;

    [Header("스킬 쿨타임 및 MP 관련")]
    public Image skillimg = null;
    public Image dashimg = null;
    public Image mpimg = null;
    public bool skillcool = false, dashcool = false;
    public Text skillText = null;

    [Header("HP,MP UI")]
    public Image hpBar;
    public Image mpBar;
    public Image hpPotion;
    public Image mpPotion;
    public Text hpPotionNum;
    public Text mpPotionNum;
    public bool hpchk = false;
    public bool mpchk = false;

    [Header("이동속도")]
    [Tooltip("기본이동속도")]
    public float moveSpeed = 6.0f;
    public Rigidbody rbody;
    public float v = 0f;
    public float h = 0f;

    [Header("애니메이션관련속성")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip WalkAnimClip = null;
    public AnimationClip RunAnimClip = null;
    public AnimationClip Attack1AnimClip = null;
    public AnimationClip Attack2AnimClip = null;
    public AnimationClip Attack3AnimClip = null;
    public AnimationClip Attack4AnimClip = null;
    public AnimationClip SkillAnimClip = null;//스킬 애니메이션 클립.
    public AnimationClip DashSkillAnimClip = null;//스킬 애니메이션 클립.
    public AnimationClip DieAnimClip = null;//스킬 애니메이션 클립.

    private Animation myAnimation = null;

    [Header("아이템 줍기")]
    public InventoryCtrl inventoryCtrl;

    [Header("음악 효과")]
    public AudioClip attackAudio = null;
    public AudioClip skillAudio = null;

    [Header("게임종료")]
    public Image gameOverImg = null;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        myAnimation = GetComponent<Animation>();
        myAnimation.playAutomatically = false;  //자동재생끄고
        myAnimation.Stop();           //애니메이션 정지
        myAnimation[WalkAnimClip.name].wrapMode = WrapMode.Loop;//걷기
        myAnimation[RunAnimClip.name].wrapMode = WrapMode.Loop;//뛰기
        myAnimation[Attack1AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack2AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack3AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack4AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[SkillAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DashSkillAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DieAnimClip.name].wrapMode = WrapMode.Once;
        

        AddAnimationEvent(Attack1AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack2AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack3AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack4AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(SkillAnimClip, "SkillAnimFinished");
        AddAnimationEvent(DashSkillAnimClip, "DashSkillAnimFinished");
        AddAnimationEvent(DieAnimClip, "DieAnimFinished");

        attackAudio = Resources.Load<AudioClip>("Sounds/sword_throw");
        skillAudio = Resources.Load<AudioClip>("Sounds/Big_Explosion_Distant2");
        hpcur = User.Instance.hp;
        mpcur = User.Instance.mp;


    }                                         
    void FixedUpdate()
    {
        Move();
        AnimationControl();//애니메이션 상태에 맞춰서 재생된다.
        CheckState();//조건에 맞추어 캐릭터 상태를 변경시켜줍니다.
        //InputControl();//마우스 왼쪽 버튼 클릭으로 공격 상태로 변경시켜 줍니다. 연속공격
        AttackComponentControl();//공격컴포넌트 제어
        DashMove();//대쉬 스킬 제어
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            inventoryCtrl.AcquireItem(other.gameObject.GetComponent<Itemgem>());
            Destroy(other.gameObject);
        }
    }

    ///상태를 변경해주는 함수 
    void CheckState()
    {
        //Vector3 speed = rbody.velocity;
        switch (myState)
        {

            case PlayerState.Idle:
                if (h != 0 || v != 0)
                {
                    myState = PlayerState.Walk;
                }
                break;
            case PlayerState.Walk:
                if (h == 0 && v == 0)
                {
                    myState = PlayerState.Idle;
                }else if (Math.Abs(h) > 0.4 || Math.Abs(v) > 0.4)
                {
                    myState = PlayerState.Run;
                }
                break;
            case PlayerState.Run:
                if (Math.Abs(h) <= 0.4 || Math.Abs(v) <= 0.4)
                {
                    myState = PlayerState.Walk;
                }
                if (h == 0 && v == 0)
                {
                    myState = PlayerState.Idle;
                }
                    break;
            case PlayerState.Attack:
                CannotMove = true;
                break;
            case PlayerState.Skill:
                CannotMove = true;
                break;
        }
    }

    #region 
    /// <summary>
    /// 이동관련 함수
    /// </summary>
    /// <param name="stickPos"></param>
    public void OnStickChanged(Vector3 stickPos)
    {
        h = -stickPos.x;
        v = -stickPos.y;

    }

    /// <summary>
    /// 이동관련 함수
    /// </summary>
    public void Move()
    {
        if (CannotMove)//이동불가일때는 함수 리턴
        {
            return;
        }
        Vector3 speed = rbody.velocity;
        //rbody 의 힘과 방향을 speed에 대입
        speed.x = moveSpeed * h;
        speed.z = moveSpeed * v;
        rbody.velocity = speed;
        if (h != 0 || v != 0)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(h, 0f, v));

        }
    }
    #endregion

    /// <summary>
    /// 애니메이션을 재생시키는 함수
    /// </summary>
    /// <param name="clip"></param>
    void AnimationPlay(AnimationClip clip)
    {
        myAnimation.clip = clip;
        myAnimation.CrossFade(clip.name);
    }

    void AnimationControl()
    {
        switch (myState)
        {
            case PlayerState.Idle:
                AnimationPlay(IdleAnimClip);
                break;
            case PlayerState.Walk:
                AnimationPlay(WalkAnimClip);
                break;
            case PlayerState.Run:
                AnimationPlay(RunAnimClip);
                break;
            case PlayerState.Attack:
                //공격상태에 맞춘 애니메이션을 재생시켜줍니다.
                AttackAnimationControl();
                break;
            case PlayerState.Skill:
                AnimationPlay(SkillAnimClip);
                break;
            case PlayerState.DashSkill:
                AnimationPlay(DashSkillAnimClip);
                break;
            case PlayerState.Die:
                break;

        }
    }

    /// <summary>
    /// 애니메이션 클립 재생이 끝날때 쯤 애니메이션 이벤트 함수를 호출 시켜주도록 추가합니다. 
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="FuncName"></param>
    void AddAnimationEvent(AnimationClip clip, string FuncName)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = FuncName;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }

    /// <summary>
    /// 공격을 합니다.
    /// </summary>
    public void Attack()
    {
        //내가 공격중이 아니라면 공격을 시작하게 되고
        if (myState != PlayerState.Attack)
        {
            myState = PlayerState.Attack;
            AttackState = FighterAttackState.Attack1;
        }
        else
        {   //공격 중이라면 애니메이션이 일정 이상 재생이 되었다면 다음 공격을 활성화
            switch (AttackState)
            {
                case FighterAttackState.Attack1:
                    if (myAnimation[Attack1AnimClip.name].normalizedTime > 0.1f)
                    {
                        NextAttack = true;
                    }
                    break;
                case FighterAttackState.Attack2:
                    if (myAnimation[Attack2AnimClip.name].normalizedTime > 0.1f)
                    {
                        NextAttack = true;
                    }
                    break;
                case FighterAttackState.Attack3:
                    if (myAnimation[Attack3AnimClip.name].normalizedTime > 0.1f)
                    {
                        NextAttack = true;
                    }
                    break;
                case FighterAttackState.Attack4:
                    if (myAnimation[Attack4AnimClip.name].normalizedTime > 0.1f)
                    {
                        NextAttack = true;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 스킬을 사용하는 함수
    /// </summary>
    public void OnSkill()
    {
        if (skillcool||mpcur<20)//쿨타임 중이거나 마나가 모자르다면
        {
            skillText.gameObject.SetActive(true);
            Invoke("TextDisable", 1.5f);
            return;
        }
        if (myState == PlayerState.Attack)
        {
            AttackState = FighterAttackState.Attack1;
            NextAttack = false;
        }
        myState = PlayerState.Skill;
        MpUse();
        StartCoroutine(SkillCoolTime(5f));
    }


    public void OnDashSkill()
    {
        if (CannotMove)//이동불가일때는 함수 리턴
        {
            return;
        }
        if (dashcool||mpcur < 20)//쿨타임 중이거나 마나가 모자르다면
        {
            skillText.gameObject.SetActive(true);
            Invoke("TextDisable", 1.5f);
            return;
        }

        if (myState == PlayerState.Attack)
        {
            AttackState = FighterAttackState.Attack1;
            NextAttack = false;
        }
        rbody.constraints = RigidbodyConstraints.FreezePositionY| RigidbodyConstraints.FreezeRotation;
        DashMoving = true;
        myState = PlayerState.DashSkill;
        MpUse();
        StartCoroutine(DashCoolTime(5f));
    }

    /// <summary>
    /// 대쉬스킬 사용시 이펙트
    /// </summary>
    public void OnDashEffect()
    {
        Vector3 position = transform.position;
        position += transform.up * 0.3f;
        Instantiate(DashEffect, position, Quaternion.identity);
    }
    /// <summary>
    /// 대쉬스킬 사용시 이동하게 변경
    /// </summary>
    public void DashMove()
    {
        
        if (!DashMoving)
        {
            return;
        }
        time += Time.deltaTime;
        if (time < 0.6f)
        {
        Vector3 speed = rbody.velocity;
        //rbody 의 힘과 방향을 speed에 대입
        rbody.velocity =speed+ transform.forward * 5f;
        }
        else{
            time = 0f;
            DashMoving = false;
        }
    }

    IEnumerator SkillCoolTime(float cool)
    {
        skillcool = true;
        float temp = 0.0f;
        while (temp <= cool)
        {
            temp += Time.deltaTime;
            skillimg.fillAmount = (temp / cool);
            yield return new WaitForFixedUpdate();
        }
        skillcool = false;
    }

    IEnumerator DashCoolTime(float cool)
    {
        
        dashcool = true;
        float temp = 0.0f;
        while (temp <= cool)
        {
            temp += Time.deltaTime;
            dashimg.fillAmount = (temp / cool);
            yield return new WaitForFixedUpdate();
        }
        dashcool = false;
    }

    public void TextDisable()
    {
        skillText.gameObject.SetActive(false);
    }

    //공격 애니메이션 재생이 끝나면 호출되는 애니메이션 이벤트 함수
    void OnAttackAnimFinished()
    {
        if (NextAttack)
        {
            NextAttack = false;
            switch (AttackState)
            {
                case FighterAttackState.Attack1:
                    AttackState = FighterAttackState.Attack2;
                    break;
                case FighterAttackState.Attack2:
                    AttackState = FighterAttackState.Attack3;
                    break;
                case FighterAttackState.Attack3:
                    AttackState = FighterAttackState.Attack4;
                    break;
                case FighterAttackState.Attack4:
                    AttackState = FighterAttackState.Attack1;
                    break;
            }
        }
        else
        {
            CannotMove = false;
            myState = PlayerState.Idle;
            AttackState = FighterAttackState.Attack1;
        }
    }

    //스킬 애니메이션이 끝났으면
    void SkillAnimFinished()
    {
        Vector3 position = transform.position;
        position += transform.forward * 2.0f;
        Instantiate(SkillEffect, position, Quaternion.identity);
        CannotMove = false;
        myState = PlayerState.Idle;
    }

    /// <summary>
    /// 대쉬스킬 애니메이션이 끝났으면
    /// </summary>
    void DashSkillAnimFinished()
    {
        rbody.constraints = RigidbodyConstraints.FreezeRotation;
        CannotMove = false;
        myState = PlayerState.Idle;
    }


    /// <summary>
    /// 공격 애니메이션을 재생합니다.
    /// </summary>
    void AttackAnimationControl()
    {
        switch (AttackState)
        {
            case FighterAttackState.Attack1:
                AnimationPlay(Attack1AnimClip);
                break;
            case FighterAttackState.Attack2:
                AnimationPlay(Attack2AnimClip);
                break;
            case FighterAttackState.Attack3:
                AnimationPlay(Attack3AnimClip);
                break;
            case FighterAttackState.Attack4:
                AnimationPlay(Attack4AnimClip);
                break;
        }
    }

    /// <summary>
    /// 공격 관련 컴포넌트 제어.
    /// </summary>
    void AttackComponentControl()
    {
        
        switch (myState)
        {
            //공격중일때만 트레일 컴포넌트 
            case PlayerState.Attack:
            case PlayerState.Skill:
                AttackTrailRenderer.enabled = true;
                break;
            case PlayerState.DashSkill:
                AttackTrailRenderer.enabled = true;
                break;
            default:
                AttackTrailRenderer.enabled = false;
                AttackBoxCollider.enabled = false;
                break;
        }

    }
    /// <summary>
    /// 프레임 단위로 충돌 컴포넌트 활성화
    /// </summary>
    void AttackControl()
    {
        //충돌 컴포넌트 활성화
        AttackBoxCollider.enabled = true;
    }

    /// <summary>
    /// 프레임 단위로 충돌 컴포넌트 활성화
    /// </summary>
    IEnumerator AttackColloder()
    {
        AttackBoxCollider.enabled = true;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        AttackBoxCollider.enabled = false;
    }

    /// <summary>
    /// 공격시 효과음악 재생
    /// </summary>
    void AttackSound()
    {
        SoundControl.Instance.PlayerSfx(transform.position, attackAudio);
    }

    /// <summary>
    /// 스킬시 효과음악 재생
    /// </summary>
    void SkillSound()
    {
        SoundControl.Instance.PlayerSfx(transform.position, skillAudio); 
    }

    //Hp 포션
    public void HpUp()
    {
        if (hpchk||int.Parse(hpPotionNum.text) <= 0|| hpcur >= User.Instance.hp)
        {
            return;
        }
        hpPotionNum.text = (int.Parse(hpPotionNum.text) - 1).ToString();
        StartCoroutine(HpPotionCool(10.0f, hpPotion));
    }

    IEnumerator HpPotionCool(float cool, Image bar)
    {
        hpchk = true;
        float temp = 0.0f;
        while (temp <= cool)
        {
            temp += Time.deltaTime;
            if (hpcur <= User.Instance.hp)
            {
                hpcur += 10 * Time.deltaTime;
                hpBar.fillAmount = (hpcur / User.Instance.hp);
            }
            bar.fillAmount = (temp / cool);
            yield return new WaitForFixedUpdate();
        }
        hpchk = false;
    }

    //Mp 포션
    public void MpUp()
    {
        if (mpchk|| int.Parse(mpPotionNum.text)<=0 || mpcur >= User.Instance.mp)
        {
            return;
        }
        mpPotionNum.text = (int.Parse(mpPotionNum.text) - 1).ToString();
        StartCoroutine(MpPotionCool(10.0f, mpPotion));
    }

    IEnumerator MpPotionCool(float cool, Image bar)
    {
        mpchk = true;
        float temp = 0.0f;
        while (temp <= cool)
        {
            temp += Time.deltaTime;
            if (mpcur <= User.Instance.mp)
            {
                mpcur += 7 * Time.deltaTime;
                mpBar.fillAmount = (mpcur / User.Instance.mp);
            }
            bar.fillAmount = (temp / cool);
            yield return new WaitForFixedUpdate();
        }
        mpchk = false;
    }

    //mp 소모
    public void MpUse()
    {
        mpcur -= 20;
        mpBar.fillAmount = (mpcur / User.Instance.mp);
    }

    //hp 소모
    public void HpMinus(float minus)
    {
        hpcur -= minus;
        hpBar.fillAmount = (hpcur / User.Instance.hp);
        DamageTweenEffect();
        if(hpcur <= 0f)
        {
            Die();
        }
    }

    public void DamageTweenEffect()
    {
        //트윈이 재생중이면 중복 트위닝 세팅하지 않습니다.
        if (effectTweener != null && !effectTweener.isComplete)
        {
            return;
        }
        Color colorTo = Color.black;
        effectTweener = HOTween.To(hpImage, 0.1f, new TweenParms().Prop("color", colorTo).Loops(1, LoopType.Yoyo).OnStepComplete(DamageTweenFinished));
    }

    void DamageTweenFinished()
    {
        hpImage.color = Color.red;
    }

    /// <summary>
    /// 죽었을때
    /// </summary>
    public void Die()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        myState = PlayerState.Die;
        AnimationPlay(DieAnimClip);
        GetComponent<CapsuleCollider>().enabled = false;

    }

    /// <summary>
    /// 대쉬스킬 애니메이션이 끝났으면
    /// </summary>
    void DieAnimFinished()
    {
        gameOverImg.gameObject.SetActive(true);
        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        time = 0f;
        Color fadeColor = gameOverImg.color;
        while (fadeColor.a < 1f)
        {
            time += Time.deltaTime / 1.2f;
            fadeColor.a = Mathf.Lerp(0f, 1f, time);
            gameOverImg.color = fadeColor;
            yield return null;
        }
        Time.timeScale = 0f;
        yield return null;
    }
}
