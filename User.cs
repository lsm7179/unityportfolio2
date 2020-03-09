using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class User : SingletonMonobehavior<User>
{
    // 힘, 이동속도, 마법력, 경험치, 레벨
    public float hp = 500f;
    public float mp = 300f;
    public int speed = 0;
    public int attack = 0;
    public int magic = 0;
    public int exp = 0;
    public int level = 1;
    public int point = 0;

    [Header("스탯 경험치")]
    public Text levelText;
    public Image expImg;
    public Text pointText;
    public Text attackText;
    public Text magicText;
    public Text speedText;
    public Image attackImg;
    public Image magicImg;
    public Image speedImg;
    public Text skillText;
    public Text dashText;


    public int[] levelexp = { 100, 200, 300, 400, 500 };

    protected override void Awake()
    {
        
    }

    /// <summary>
    /// 레벨을 체크하여 정리
    /// </summary>
    public void CheckLevel()
    {
        if(level >= 5)
        {
            return;
        }
        int MaxExp = levelexp[level - 1];
        if (MaxExp <= exp)
        {
            level++;
            levelText.text = level.ToString();
            exp = 0;
            expImg.fillAmount = 0f;
            point += 2;
            pointText.text = point.ToString();
            attackImg.gameObject.SetActive(true);
            magicImg.gameObject.SetActive(true);
            speedImg.gameObject.SetActive(true);
            if (level >= 5)
            {
                expImg.fillAmount = 1f;
            }
        }
    }

    /// <summary>
    /// 공격했을때 데미지 입히게 하기
    /// </summary>
    public int AttackDamage()
    {
        int damage = 50 + User.Instance.attack * 10; ;
        if (PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.Attack)
        {
        return damage;
        }
        else if (PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.DashSkill|| PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.Skill)
        {
         damage = 70 + User.Instance.magic * 12;
        }
        
        return damage;
    }

    public void PointAttack()
    {
        if (point <= 0)
        {
            return;
        }
        point--;
        attack++;
        pointText.text = point.ToString();
        attackText.text = attack.ToString();
        if (point <= 0)
        {
            attackImg.gameObject.SetActive(false);
            magicImg.gameObject.SetActive(false);
            speedImg.gameObject.SetActive(false);
        }
    }

    public void PointMagic()
    {
        if (point <= 0)
        {
            return;
        }
        point--;
        magic++;
        pointText.text = point.ToString();
        magicText.text = magic.ToString();
        int magicdamage = 50 + magic * 5;
        skillText.text = "전방에 화염 폭탄을 만들어 "+ magicdamage + "의 데미지를 줍니다."
                            + "\r\n쿨타임: 5 MP소모량: 20";

        dashText.text = "대쉬하며 " + magicdamage + "의 데미지를 줍니다."
                            + "\r\n쿨타임: 5 MP소모량: 20";
        if (point <= 0)
        {
            attackImg.gameObject.SetActive(false);
            magicImg.gameObject.SetActive(false);
            speedImg.gameObject.SetActive(false);
        }
    }

    public void PointSpeed()
    {
        if (point <= 0)
        {
            return;
        }
        point--;
        speed++;
        pointText.text = point.ToString();
        speedText.text = speed.ToString();
        if (point <= 0)
        {
            attackImg.gameObject.SetActive(false);
            magicImg.gameObject.SetActive(false);
            speedImg.gameObject.SetActive(false);
        }
    }
}
