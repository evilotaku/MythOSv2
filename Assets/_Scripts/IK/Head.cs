using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    public Transform rootObject, followObject;
    public Vector3 _headBodyOffset, positionOffset, rotationOffset;

    private void Start()
    {
        if (!followObject) return;
        //_headBodyOffset = transform.position - followObject.position;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (!followObject) return;
        //update body position/rotation
        rootObject.position = transform.position + _headBodyOffset;
        rootObject.forward = Vector3.ProjectOnPlane(followObject.forward, Vector3.up).normalized;

        //update head position/rotation
        transform.position = followObject.TransformPoint(positionOffset);
        transform.rotation = followObject.rotation * Quaternion.Euler(rotationOffset);
    }
}
