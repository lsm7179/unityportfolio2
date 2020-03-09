using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    //카메라 기본 속성
    [Header("카메라 기본 속성")]
    private Transform myTransform = null;
    private Transform targetTransform = null;
    public Transform lookAtTransform = null;

    [Header("3인칭 카메라")]
    public float distance = 5.5f;
    public float Height = 1.5f; //타겟의 위치보다 더 추가적인 높이
    public float HeightDamping = 2.0f;  //speed 라고 생각하면 된다.
    public float RotationDamping = 3.0f; //y축 속도
    void Start()
    {
        myTransform = GetComponent<Transform>();
        targetTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    /// <summary>
    /// 3인칭 카레마
    /// </summary>
    void ThirdView()
    {
        float wantedRotationAngle = targetTransform.eulerAngles.y;//현재 타겟의 y축 각도값
        float wantedHeight = targetTransform.position.y + Height;//현재 타겟의 높이 + 우리가 추가로 높이고 싶은 높이
        float currentRotationAngle = myTransform.eulerAngles.y;//현재 카메라의 y축 각도값
        float currentHeight = myTransform.position.y;//현재 카메라의 높이값

        //현재 각도에서 원하는 각도로 댐핑값을 얻게 됩니다.
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, RotationDamping * Time.deltaTime);

        //현재 높이에서 원하는 높이로 댐핑값을 얻게 됩니다.
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, HeightDamping * Time.deltaTime);

        //회전각도Rotation을 반환합니다.
        Quaternion currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);

        myTransform.position = targetTransform.position;
        //카메라 포지션에 넣어준다.
        myTransform.position -= currentRotation * Vector3.forward * distance;
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        //myTransform.LookAt(lookAtTransform);
    }

    void LateUpdate()
    {
        ThirdView();
    }
}
