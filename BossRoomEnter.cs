using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossRoomEnter : MonoBehaviour
{
    public Image bossHp;
    public bool roomexit = false;
    public BossCtrl bossCtrl=null;
    public bool timechk = true;
    public Collider collider = null;
    /*
    보스전 진입 하면 캠퍼스 나오게 하고 보스상태 변화
    보스전 나가면 캠퍼스 사라지고 보스 idle로 변화
    다시 들어갈땐 풀피 만들기 쉽죠?
    */
    void Start()
    {
        bossCtrl = BossCtrl.Instance;
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            roomexit = !roomexit;
            roominexit(roomexit);
            collider.enabled = false;
        }
    }

    IEnumerator timer()
    {
        yield return new WaitForSeconds(3.0f);
        collider.enabled = true;
    }

    public void roominexit(bool chk)
    {
        if (chk)
        {
            if (!bossHp.gameObject.activeInHierarchy)
            {
                bossHp.fillAmount=1.0f;
                bossHp.gameObject.SetActive(true);
            }
            bossCtrl.bossState = BossCtrl.BossState.trace;
        }
        else
        {
            if (bossHp.gameObject.activeInHierarchy)
            {
                bossHp.gameObject.SetActive(false);
            }
            bossCtrl.bossState = BossCtrl.BossState.back;
        }
        StartCoroutine(timer());
    }
}
