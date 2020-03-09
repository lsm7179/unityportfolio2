using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {

    private Transform mainCamTr;
    private Transform thisCanvasTr;

	void Start () {
        mainCamTr = Camera.main.transform;
        thisCanvasTr = GetComponent<Transform>();
	}
    
    void LateUpdate () {
        
        //y축으로만 바라보게 만들기.
        thisCanvasTr.rotation = Quaternion.Euler(mainCamTr.rotation.x, mainCamTr.rotation.y, mainCamTr.rotation.z);
        //thisCanvasTr.LookAt(mainCamTr);
	}
}
