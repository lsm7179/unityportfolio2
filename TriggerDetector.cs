using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public string Tag = string.Empty;

    private void OnTriggerEnter(Collider other)
    {
        //이 메소드를 사용하는게 좋은 방법입니다.
        if (other.gameObject.CompareTag("Player"))
        {
            //부모 스크립트에 메세지를 보낸다.
            gameObject.SendMessageUpwards("OnSetTarget", SendMessageOptions.DontRequireReceiver);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //이 메소드를 사용하는게 좋은 방법입니다.
        if (other.gameObject.CompareTag("Player"))
        {
            //부모 스크립트에 메세지를 보낸다.
            gameObject.SendMessageUpwards("OnSetTargetCancel", SendMessageOptions.DontRequireReceiver);
        }
    }
}
