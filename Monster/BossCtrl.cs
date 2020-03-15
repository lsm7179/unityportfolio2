using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Holoville.HOTween;

public class BossCtrl : SingletonMonobehavior<BossCtrl>
{

    // 우선 상태 로직 및 캠퍼스 공격상태 로직을 구현시키자 Update에 넣을것
    // 공격은 콜라이더를 이용하지 않고 거리 계산으로 해보자
    [Header("기본속성")]
    public float rotSpeed = 5.0f;//회전 스피드
    public float runSpeed = 11f;//추적 속도
    public float TraceDist = 11f;//
    public float AttackRange = 4f;
    public Transform bossTr;
    public Transform playerTr;
    public Vector3 bosshomeTr;
    public BossAttackRange bossAttackRange;
    public GameObject bossroom;


    [Header("보스 상태")]
    public BossState bossState = BossState.idle;
    public enum BossState { idle = 0, trace, back, attack, damage, jumpattack, die };

    public Animator ani;
    public bool attackcombo = false;
    public int jumbAttackCount = 0;//점프 공격 체크
    public int attackCount = 0;//공격콤보 체크

    [Header("HP EXP")]
    [SerializeField]
    protected Image hpBar;
    public int HpInit = 1000;
    public int Hp = 1000;
    public int expgive = 100;//경험치 주는 양
    

    [Header("이펙트")]
    [SerializeField]
    protected GameObject hitEffect = null;
    [SerializeField]
    protected GameObject DestroyEffect = null;
    [SerializeField]
    protected GameObject hpEffect = null;
    public GameObject bossGem = null;
    protected Tweener effectTweener = null;
    public SkinnedMeshRenderer skinMeshRenderer = null;
    public Color skinColor;
    public GameObject jumpAttackParticle = null;

    [Header("사운드")]
    public AudioClip jumpclip;
    public AudioClip hitclip;
    public AudioClip dieSound = null;

    void Start()
    {
        ani = GetComponent<Animator>();
        bossTr = GetComponent<Transform>();
        bosshomeTr = bossTr.position;
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        hitEffect = Resources.Load<GameObject>("Prefabs/Particle/HitEffect");
        hpEffect = Resources.Load<GameObject>("Prefabs/Particle/HPParticle");
        DestroyEffect = Resources.Load<GameObject>("Prefabs/Particle/DestroyEffect");
        jumpclip = Resources.Load<AudioClip>("Sounds/Enemy/enemy_FlyingBuzzer_DestroyedExplosion");
        hitclip = Resources.Load<AudioClip>("Sounds/Bosshit");
        skinColor = skinMeshRenderer.material.color;
        bossAttackRange=GetComponentInChildren<BossAttackRange>();


    }

    void FixedUpdate()
    {
        CheckState();
        AnimationControl();
        
    }

    public void CheckState()
    {
        switch (bossState)
        {
            case BossState.idle://idle일때는 그냥 가만히 있는다.
                break;
            case BossState.trace:// 플레이어에게 다가간다. attack 또는 idle
            case BossState.back:
                TraceUpdate();
                break;
            case BossState.attack://attack -> damage, die , trace
                AttackUpdate();
                break;
            case BossState.jumpattack:
                JumpAttackUpdate();
                break;
            case BossState.damage:
                break;
            case BossState.die:
                break;
            default:
                break;

        }
    }

    public void AnimationControl()
    {
        switch (bossState)
        {
            case BossState.idle:
                ani.SetBool("IsWalk", false);
                ani.SetBool("JumpAttack", false);
                ani.SetBool("combo", attackcombo);
                break;
            case BossState.back:
            case BossState.trace:
                ani.SetBool("IsWalk", true);
                ani.SetBool("JumpAttack", false);
                ani.SetBool("combo", attackcombo);
                break;
            case BossState.attack:
                ani.SetBool("IsWalk", false);
                ani.SetBool("JumpAttack", false);
                ani.SetBool("combo", attackcombo);
                break;
            case BossState.jumpattack:
                ani.SetBool("IsWalk", false);
                ani.SetBool("JumpAttack", true);
                ani.SetBool("combo", attackcombo);
                break;
            case BossState.damage:
                break;
            case BossState.die:
                
                break;
        }
    }

    /// <summary>
    /// 플레이어 추척상태일때의 동작
    /// </summary>
    public void TraceUpdate()
    {
        if(bossState == BossCtrl.BossState.trace)
        {
            Vector3 diff = playerTr.position- bossTr.position;
            Vector3 direction = diff.normalized;
            direction = new Vector3(direction.x, 0.0f, direction.z);
            Vector3 moveAmount = direction * runSpeed * Time.deltaTime;
            moveAmount.y = 0.0f;
            if(diff.magnitude < AttackRange)
            {
                bossState = BossState.attack;
                return;
            }
            bossTr.Translate(moveAmount, Space.World);
            bossTr.LookAt(playerTr.position);//바라보게 하기
        }
        else if(bossState == BossCtrl.BossState.back)
        {
            Vector3 diff = bosshomeTr - bossTr.position;
            Vector3 direction = diff.normalized;
            direction = new Vector3(direction.x, 0.0f, direction.z);
            Vector3 moveAmount = direction * runSpeed * Time.deltaTime;
            moveAmount.y = 0.0f;
            bossTr.Translate(moveAmount, Space.World);
            Vector3 look = Vector3.Slerp(bossTr.forward, diff.normalized, Time.deltaTime);
            bossTr.rotation = Quaternion.LookRotation(look, Vector3.up);
            if (diff.magnitude < 2f)
            {
                bossState = BossState.idle;
                return;
            }
        }
    }


    /// <summary>
    /// 공격상태
    /// </summary>
    public void AttackUpdate()
    {
        Vector3 diff = playerTr.position - bossTr.position;
        bossTr.LookAt(playerTr.position);
        if (!attackcombo)//최초실행
        {
            ani.SetTrigger("AttackStart");
            attackcombo = true;
        }

        if(diff.magnitude > AttackRange)
        {
            attackcombo = false;
            bossState = BossState.trace;
        }
    }
    /// <summary>
    /// 점프공격상태
    /// </summary>
    public void JumpAttackUpdate()
    {
        if (jumbAttackCount >= 3)
        {
            jumbAttackCount = 0;
            bossState = BossState.trace;
        }
    }

    /// <summary>
	/// 피격 되었는지 확인 하는 함수
	/// </summary>
	/// <param name="other"></param>
	public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sword"))
        {
            MinusHp(other.transform.position);
        }
    }

    /// <summary>
    /// 맞았을때 처리
    /// </summary>
    /// <param name="hitPos"></param>
    public void MinusHp(Vector3 hitPos)
    {
        int damage = User.Instance.AttackDamage();
        GameObject hitEff = Instantiate(hitEffect, hitPos, Quaternion.identity);
        TextMesh hptext = hpEffect.GetComponentInChildren<TextMesh>();
        hptext.text = damage.ToString();
        GameObject hpEff = Instantiate(hpEffect, hitPos, Quaternion.identity);

        Hp -= damage;
        hpBar.fillAmount = (float)Hp / (float)HpInit;
        Destroy(hitEff, 0.5f);
        if (Hp <= 0 && bossState != BossState.die)//죽었을때
        {
            Die();
        }
        
    }

    /// <summary>
    /// 스킬로 데미지 줄때
    /// </summary>
    public void SkillHit()
    {
        int damage = User.Instance.AttackDamage();
        TextMesh hptext = hpEffect.GetComponentInChildren<TextMesh>();
        hptext.text = damage.ToString();
        GameObject hpEff = Instantiate(hpEffect, this.transform.position, Quaternion.identity);
        Hp -= damage;
        hpBar.fillAmount = (float)Hp / (float)HpInit;
        if (Hp <= 0 && bossState != BossState.die)//죽었을때
        {
            Die();
        }
    }

    /// <summary>
    /// die -> 오브젝트풀 -> wait ->idle
    /// 캔버스 , hp 초기화, 내비게이션 초기화 경험치 획득
    /// </summary>
    public void Die()
    {
        bossState = BossState.die;
        attackcombo = false;
        ani.SetBool("IsWalk", false);
        ani.SetBool("JumpAttack", false);
        ani.SetBool("combo", attackcombo);
        ani.SetTrigger("IsDie");
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation
            | RigidbodyConstraints.FreezePositionY;
        GetComponent<CapsuleCollider>().enabled = false;
        if (User.Instance.level >= 5)
        {
            return;
        }
        User.Instance.expPlus(expgive);
        bossroom.SetActive(false);
    }


/*애니메이션 이벤트 함수 시작---------------------------------------------------------------------------------------------------*/

    /// <summary>
    /// 죽는 애니메이션 끝나고 나서 행동 , 아이템 드롭, 몬스터 사라지게 하기
    /// </summary>
    public void DieEffect()
    {
        GameObject DestoryEffect_ = Instantiate(DestroyEffect, bossTr.position, Quaternion.identity);
        GameObject bossGem_ = (GameObject)Instantiate(bossGem, new Vector3(bossTr.position.x, bossTr.position.y + 1.6f, bossTr.position.z), Quaternion.Euler(-90f, 0f, 0f));
        Destroy(DestoryEffect_, 0.5f);
        SoundControl.Instance.PlayerSfx(bossTr.position, dieSound);
        this.gameObject.SetActive(false);
        hpBar.gameObject.SetActive(false);
    }
    public void AttackSound()
    {
        SoundControl.Instance.PlayerSfx(bossTr.position, hitclip, 1.0f);
        //공격범위 안에 들어왔다면
        if (bossAttackRange.AttackRangeChk())
        {
            PlayerCtrl.Instance.HpMinus(10.0f);
        }
        
    }

    public void Attack3_Sound()
    {
        SoundControl.Instance.PlayerSfx(bossTr.position, jumpclip, 1.0f);
        //공격범위 안에 들어왔다면
        if (bossAttackRange.AttackRangeChk())
        {
            PlayerCtrl.Instance.HpMinus(15.0f);
        }
    }

    /// <summary>
    /// 공격을 체크하여 점프공격 상태로 바꾼다. 
    /// 트위너를 이용해서 몸이 3번정도 깜빡임과 빨간색으로 바뀐다.
    /// </summary>
    public void AttackCountCheck()
    {
        if (++attackCount >= 2)
        {
            bossState = BossState.jumpattack;
            attackCount = 0;
            attackcombo = false;
            DamageTweenEffect();
        }
    }

    public void DamageTweenEffect()
    {
        //트윈이 재생중이면 중복 트위닝 세팅하지 않습니다.
        if (effectTweener != null && !effectTweener.isComplete)
        {
            return;
        }
        Color colorTo = Color.red;
        effectTweener = HOTween.To(skinMeshRenderer.material, 0.4f, new TweenParms().Prop("color", colorTo).Loops(5, LoopType.Yoyo).OnStepComplete(DamageTweenFinished));
    }

    void DamageTweenFinished()
    {
        skinMeshRenderer.material.color = skinColor;
    }

    public void JumpAttackLook()
    {
        bossTr.LookAt(playerTr.position);//바라보게 하기
    }

    /// <summary>
    /// 점프공격 3번 정도 하면 되돌아 가야된다.
    /// </summary>
    public void JumpAttackCount()
    {
        SoundControl.Instance.PlayerSfx(bossTr.position, jumpclip, 1.0f);
        jumpAttackParticle.SetActive(true);
        jumbAttackCount++;
    }

    /*애니메이션 이벤트 함수끝---------------------------------------------------------------------------------------------------*/
}
