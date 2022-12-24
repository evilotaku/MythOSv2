using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteadyCam : MonoBehaviour
{

    public GameObject followObj;
    public float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    
  
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, followObj.transform.position, ref velocity, smoothTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, followObj.transform.rotation, Time.deltaTime * smoothTime);
    }
}
