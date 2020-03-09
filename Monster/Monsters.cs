using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Holoville.HOTween;

//상속을 이용한 스크립트 소스 작성
public class Monsters : MonoBehaviour
{
	[Header("기본속성")]
	public float rotSpeed = 5.0f;
	public float TraceDist = 11f;
	public float AttackRange = 1.5f;
	public Transform monsterTr;
	public bool TargetPlayer = false;
	public Transform PlayerTr = null;
	public Transform TargetTransform = null;
	public Vector3 TargetPosition = Vector3.zero;
	[SerializeField]
	protected NavMeshAgent Navi;
	[SerializeField]
	protected Animator Ani;
	[Header("HP EXP")]
	[SerializeField]
	protected Image hpBar;
	[SerializeField]
	protected Canvas thisCanvas;
	public int HpInit = 130;
	public int Hp = 130;
	public int expgive = 25;//경험치 주는 양

	[Header("이펙트")]
	[SerializeField]
	protected GameObject hitEffect = null;
	[SerializeField]
	protected GameObject DestroyEffect = null;
	[SerializeField]
	protected GameObject hpEffect = null;
	public GameObject monsterGem = null;
	protected Tweener effectTweener = null;
	protected SkinnedMeshRenderer skinMeshRenderer = null;
	[SerializeField]
	protected BoxCollider boxCollider = null;

	[Header("몬스터 상태")]
	public MonsterState monsterState = MonsterState.idle;
	public enum MonsterState { idle = 0, wait, patrol, trace, attack, die };
	public Vector3 startPos;

	[Header("음악효과")]
	public AudioClip dieSound = null;
	public AudioClip hitSound = null;

	[Header("데미지 주기")]
	public bool attack = false;


	protected void Awake()
	{
		monsterTr = GetComponent<Transform>();
		PlayerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
		Ani = GetComponent<Animator>();
		hpBar = GetComponentInChildren<Image>();
		thisCanvas = GetComponentInChildren<Canvas>();
		hitEffect = Resources.Load<GameObject>("Prefabs/Particle/HitEffect");
		hpEffect = Resources.Load<GameObject>("Prefabs/Particle/HPParticle");
		DestroyEffect = Resources.Load<GameObject>("Prefabs/Particle/DestroyEffect");
		Navi = GetComponent<NavMeshAgent>();
		skinMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();//스킨메쉬를 캐싱
		boxCollider = GetComponentInChildren<BoxCollider>();
	}

	protected void FixedUpdate()
	{
		CheckState();
		AnimationControl();
	}

	/// <summary>
	/// 몬스터가 active 상태일때 메소드가 실행된다. 캠퍼스 보이게 하기 hp
	/// </summary>
	protected void OnEnable()
	{
		StartCoroutine(StartPos());
		StartCoroutine(WaitUpdate());
		thisCanvas.enabled = true;
		Hp = HpInit;
		hpBar.fillAmount = 1.0f;
		GetComponent<CapsuleCollider>().enabled = true;
		GetComponentInChildren<SphereCollider>().enabled = true;
		if (boxCollider!=null)
		{
			boxCollider.enabled = true;
		}
		
	}

	IEnumerator StartPos()
	{
		yield return new WaitForSeconds(0.5f);
		startPos = monsterTr.position;
	}

	/// <summary>
	/// 몬스터가 disable 상태일때 메소드 실행
	/// </summary>
	private void OnDisable()
	{
		
	}

	/// <summary>
	/// 몬스터 상태 체크
	/// </summary>
	protected void CheckState()
	{

		switch (monsterState)
		{
			case MonsterState.idle:
				IdleUpdate();//idle -> MoveTotarget, Patrol
				break;
			case MonsterState.patrol:
			case MonsterState.trace:
				TraceUpdate();//trace -> wait, attack
				break;
			case MonsterState.attack://attack -> die, moveToTarget
				AttackUpdate();
				break;
			default:
				break;
		}

	}
	/// <summary>
	/// 애니메이션을 재생시켜주는 함수
	/// </summary>
	protected void AnimationControl()
	{
		switch (monsterState)
		{
			case MonsterState.wait:
			case MonsterState.idle:
				Ani.SetBool("IsTrace", false);
				Ani.SetBool("IsAttack", false);
				break;
			case MonsterState.patrol:
			case MonsterState.trace:
				Ani.SetBool("IsTrace", true);
				Ani.SetBool("IsAttack", false);
				break;
			case MonsterState.attack:
				Ani.SetBool("IsTrace", false);
				Ani.SetBool("IsAttack", true);
				break;
			case MonsterState.die:
				Ani.SetBool("IsTrace", false);
				Ani.SetBool("IsAttack", false);
				Ani.SetTrigger("IsDie");
				break;
		}
	}

	/// <summary>
	/// 대기 상태일때의 동작
	/// </summary>
	protected void IdleUpdate()
	{
		//랜덤으로 포인트를 만들어서 이동시킨다.
		if (!TargetPlayer)
		{
			TargetPosition = startPos;
			//상태를 정찰 상태로 변경.
			if (Vector3.Distance(monsterTr.position, startPos) <= 0.5f)
			{
				return;
			}
			monsterState = MonsterState.patrol;
		}
		else
		{
			//타겟이 있다면 타겟을 향해 이동합니다.
			monsterState = MonsterState.trace;
		}
	}

	/// <summary>
	/// 추적하는 함수
	/// </summary>
	protected void TraceUpdate()
	{
		//두 벡터의 차
		Vector3 diff = Vector3.zero;
		Vector3 lookAtPosition = Vector3.zero;
		switch (monsterState)
		{
			case MonsterState.patrol:
				if (TargetPosition != Vector3.zero)
				{
					Navi.SetDestination(TargetPosition);
					//목표지점까지 왔거나 . 경로가 오래되었을때
					if (Navi.remainingDistance <= Navi.stoppingDistance||Navi.isPathStale)
					{
						StartCoroutine(WaitUpdate());
						return;
					}
				}
				break;
			case MonsterState.trace:
				Navi.SetDestination(PlayerTr.position);
				if (Navi.remainingDistance <= Navi.stoppingDistance)
				{
					monsterState = MonsterState.attack;
					return;
				}
				break;
		}
		Navi.isStopped = false;
	}

	/// <summary>
	/// 대기하는 동작.
	/// </summary>
	/// <returns></returns>
	IEnumerator WaitUpdate()
	{
		monsterState = MonsterState.wait;
		float waitTime = Random.Range(1.0f, 3.0f);
		yield return new WaitForSeconds(waitTime);
		monsterState = MonsterState.idle;
	}

	/// <summary>
	/// 인지범위안에 다른 트리거나 플레이어가 들어왔다면 호출됩니다.
	/// </summary>
	protected void OnSetTarget()
	{
		TargetPlayer = true;
		//타겟을 향해 이동하는 상태로 전환
		monsterState = MonsterState.trace;
	}
	/// <summary>
	/// 인지범위 밖으로 플레이어가 나갔다면
	/// </summary>
	protected void OnSetTargetCancel()
	{
		TargetPlayer = false;
		//대기 - 정찰 상태로 전환
		StartCoroutine(WaitUpdate());
	}

	/// <summary>
	/// 공격 상태일때의 동작
	/// </summary>
	protected void AttackUpdate()
	{
		Navi.isStopped = true;
		transform.LookAt(Vector3.Lerp(transform.position, PlayerTr.position, Time.deltaTime*10f));

		if (Navi.remainingDistance > Navi.stoppingDistance)
		{
			//타켓과의 거리가 걸어지면 타겟으로 이동합니다.
			monsterState = MonsterState.trace;
		}
	}

	/// <summary>
	/// 피격 되었는지 확인 하는 함수
	/// </summary>
	/// <param name="other"></param>
	private void OnTriggerEnter(Collider other)
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
	protected void MinusHp(Vector3 hitPos)
	{
		int damage = User.Instance.AttackDamage();
		Ani.SetTrigger("IsHit");
		GameObject hitEff = Instantiate(hitEffect, hitPos, Quaternion.identity);
		TextMesh hptext = hpEffect.GetComponentInChildren<TextMesh>();
		hptext.text = damage.ToString();
		GameObject hpEff = Instantiate(hpEffect, hitPos, Quaternion.identity);

		Hp -= damage;
		hpBar.fillAmount = (float)Hp / (float)HpInit;

		if (Hp <= 0)//죽었을때
		{
			Die();
		}
		Destroy(hitEff, 0.5f);
	}

	/// <summary>
	/// 스킬로 데미지 줄때
	/// </summary>
	protected void SkillHit()
	{
		int damage = User.Instance.AttackDamage();
		Ani.SetTrigger("IsHit");
		TextMesh hptext = hpEffect.GetComponentInChildren<TextMesh>();
		hptext.text = damage.ToString();
		GameObject hpEff = Instantiate(hpEffect, this.transform.position, Quaternion.identity);
		Hp -= damage;
		hpBar.fillAmount = (float)Hp / (float)HpInit;

		if (Hp <= 0)//죽었을때
		{
			Die();
		}
	}

	/// <summary>
	/// die -> 오브젝트풀 -> wait ->idle
	/// 캔버스 , hp 초기화, 내비게이션 초기화
	/// </summary>
	protected void Die()
	{
		monsterState = MonsterState.die;
		thisCanvas.enabled = false;
		GetComponent<Collider>().enabled = false;
		GetComponentInChildren<SphereCollider>().enabled = false;
		if (boxCollider != null)
		{
			boxCollider.enabled = true;
		}
		if (User.Instance.level >= 5)
		{
			return;
		}
		User.Instance.exp += expgive;
		User.Instance.expImg.fillAmount = ((float)User.Instance.exp / User.Instance.levelexp[User.Instance.level - 1]);
		User.Instance.CheckLevel();
	}

	/// <summary>
	/// 죽는 애니메이션 끝나고 나서 행동 , 아이템 드롭, 몬스터 사라지게 하기
	/// </summary>
	protected void DieEffect()
	{
		GameObject DestoryEffect_ = Instantiate(DestroyEffect, monsterTr.position, Quaternion.identity);
		//monsterGem
		float gemx = monsterTr.position.x + Random.Range(-0.8f, 0.8f);
		float gemz = monsterTr.position.z + Random.Range(-0.8f, 0.8f);
		GameObject monsterGem_ = (GameObject)Instantiate(monsterGem, new Vector3(gemx, monsterTr.position.y+0.3f, gemz), Quaternion.identity);
		Destroy(DestoryEffect_, 0.5f);
		SoundControl.Instance.PlayerSfx(monsterTr.position, dieSound);
		this.gameObject.SetActive(false);
	}

	/// <summary>
	/// 몬스터가 때릴시 나오는 소리
	/// </summary>
	protected void HitSound()
	{
		SoundControl.Instance.PlayerSfx(monsterTr.position, hitSound,0.7f);
	}

	/// <summary>
	/// 몬스터가 때릴시 데미지 주게 하기
	/// </summary>
	protected void AttackDamage()
	{
		attack = true;
	}

	/// <summary>
	/// 몬스터 공격 애니메이션 끝
	/// </summary>
	protected void AttackBack()
	{
		attack = false;
	}

	public virtual void AttackCheck()
	{
		if (attack)
		{
			PlayerCtrl.Instance.HpMinus(5.0f);
			attack = false;
		}
	}
}
