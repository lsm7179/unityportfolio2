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
    public float hpcur = 500f;
    public float mpcur = 300f;

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
        //테스트용 유저 초기화
        //PlayerPrefs.DeleteAll();


        //유저 스탯 저장 불러오기
        //처음 실행 되었는지 확인
        if (!PlayerPrefs.HasKey("user"))
        {
            PlayerPrefs.SetString("user", "wow");
            PlayerPrefs.SetFloat("hp", 500f);
            PlayerPrefs.SetFloat("mp", 300f);
            PlayerPrefs.SetInt("speed", 0);
            PlayerPrefs.SetInt("attack", 0);
            PlayerPrefs.SetInt("magic", 0);
            PlayerPrefs.SetInt("exp", 0);
            PlayerPrefs.SetInt("level", 1);
            PlayerPrefs.SetInt("point", 0);
            PlayerPrefs.SetFloat("hpcur", 500f);
            PlayerPrefs.SetFloat("mpcur", 300f);

            PlayerPrefs.SetString("hpPotionNum", "10");
            PlayerPrefs.SetString("mpPotionNum", "10");


        }
        else
        {
            //불러오기
            hp = PlayerPrefs.GetFloat("hp");
            mp = PlayerPrefs.GetFloat("mp");
            speed = PlayerPrefs.GetInt("speed");
            attack = PlayerPrefs.GetInt("attack");
            magic = PlayerPrefs.GetInt("magic");
            exp = PlayerPrefs.GetInt("exp");
            level = PlayerPrefs.GetInt("level");
            point = PlayerPrefs.GetInt("point");
            hpcur = PlayerPrefs.GetFloat("hpcur");
            mpcur = PlayerPrefs.GetFloat("mpcur");

            //exp level point magic 
            expImg.fillAmount = ((float)exp / levelexp[level - 1]);
            levelText.text = level.ToString();
            pointText.text = point.ToString();
            magicText.text = magic.ToString();
            attackText.text = attack.ToString();
            speedText.text = speed.ToString();
            int magicdamage = 50 + magic * 5;
            skillText.text = "전방에 화염 폭탄을 만들어 " + magicdamage + "의 데미지를 줍니다."
                                + "\r\n쿨타임: 5 MP소모량: 20";

            dashText.text = "대쉬하며 " + magicdamage + "의 데미지를 줍니다."
                                + "\r\n쿨타임: 5 MP소모량: 20";
            if (point > 0)
            {
                attackImg.gameObject.SetActive(true);
                magicImg.gameObject.SetActive(true);
                speedImg.gameObject.SetActive(true);
            }
        }
    }

    public void SaveStat(string key, float value)
    {
        PlayerPrefs.SetFloat(key,value);
    }

    public void SaveStat(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public void SaveStat(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    /// <summary>
    /// 몬스터가 죽을때 호출됨 경험치 획득
    /// </summary>
    /// <param name="exp"></param>
    public void expPlus(int expgive)
    {
        exp += expgive;
        expImg.fillAmount = ((float)exp / levelexp[level - 1]);
        SaveStat("exp", exp);//변경사항 저장
        CheckLevel();
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

            SaveStat("level", level);//변경사항 저장
            SaveStat("point", point);//변경사항 저장

        }
    }

    /// <summary>
    /// 공격했을때 데미지 입히게 하기
    /// </summary>
    public int AttackDamage()
    {
        int damage = 50 + attack * 10;
        if (PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.Attack)
        {
        return damage;
        }
        else if (PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.DashSkill|| PlayerCtrl.Instance.myState == PlayerCtrl.PlayerState.Skill)
        {
         damage = 70 + magic * 12;
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
        SaveStat("point", point);//변경사항 저장
        SaveStat("attack", attack);//변경사항 저장
        PointEmpty();
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
        SaveStat("point", point);//변경사항 저장
        SaveStat("magic", magic);//변경사항 저장
        PointEmpty();
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
        SaveStat("point", point);//변경사항 저장
        SaveStat("speed", speed);//변경사항 저장
        PointEmpty();
    }

    /// <summary>
    /// 포인트를 다 사용했을때
    /// </summary>
    public void PointEmpty()
    {
        if (point <= 0)
        {
            attackImg.gameObject.SetActive(false);
            magicImg.gameObject.SetActive(false);
            speedImg.gameObject.SetActive(false);
        }
    }
}
