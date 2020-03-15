using UnityEngine;
using System.Collections;

public class BossJumpAttackChk : MonoBehaviour
{
    public float speed = 0.1f;
    public Transform thisTr;
    public Vector3 thisPos;
    public float range = 1.9f;
    
    public void OnEnable()
    {
        thisTr = GetComponent<Transform>();
        thisPos = thisTr.position;
        StartCoroutine(Disabled());
        StartCoroutine(AttackChk());
    }

    IEnumerator AttackChk()
    {
        bool chk = true;
        while (chk)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, range);
            foreach (Collider col in colls)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    PlayerCtrl.Instance.HpMinus(20.0f);
                    chk = false;
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Disabled()
    {
        yield return new WaitForSeconds(0.7f);
        thisTr.position = thisPos;
        this.gameObject.SetActive(false);
    }

    void Update () {
        transform.Translate(Vector3.forward * speed);
	}



}
