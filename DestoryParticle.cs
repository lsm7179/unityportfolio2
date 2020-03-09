using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryParticle : MonoBehaviour
{
    public float destroyTime = 2.0f;
    void Start()
    {
        Destroy(this.gameObject, destroyTime);
    }
}
