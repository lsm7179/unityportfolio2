using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerMoveCtrl : MonoBehaviour {


    public GameObject player;
    [Header("카메라 위치관련")]
    public float Camx = 0.0f;
    public float Camy = 7.5f;
    public float Camz = -8f;

    public Vector3 offset;
    private Transform camTr;
    public bool CamMoveChk = false;
    
    public float timecam = 0f;
	void Start () {
        offset = new Vector3(Camx, Camy, Camz);
        //offset = transform.position - player.transform.position;
        camTr = GetComponent<Transform>();
    }
	
	void LateUpdate () {

        if (!CamMoveChk)
        {
            CamMove();
            return;
        }
        camTr.position = player.transform.position + offset;
        camTr.LookAt(player.transform);
    }
    void CamMove()
    {
        if (timecam <= 1.0f)
        {
            timecam += Time.deltaTime /5f;
            Vector3 togo = Vector3.Lerp(camTr.position, (player.transform.position + offset), timecam);
            camTr.position = togo;
            camTr.LookAt(player.transform);
            return;
        }
        CamMoveChk = true;
    }
   
}
