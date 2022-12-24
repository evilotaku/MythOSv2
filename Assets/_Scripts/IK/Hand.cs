using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hand : NetworkBehaviour
{

    public float followSpeed = 30f, rotateSpeed = 100f;
    public Vector3 positionOffset, rotationOffset;
    public Transform FollowTarget;
    Rigidbody _rb;
    
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!FollowTarget) return;
        _rb = GetComponent<Rigidbody>();

        _rb.position = FollowTarget.position;
        _rb.rotation = FollowTarget.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!FollowTarget) return;
        //match position
        _rb.MovePosition(FollowTarget.position + positionOffset);
        //match rotation        
        Quaternion targetRotation = FollowTarget.rotation * Quaternion.Euler(rotationOffset) * Quaternion.Inverse(_rb.rotation);
        targetRotation.ToAngleAxis(out float angle, out Vector3 axis);
        _rb.angularVelocity = Mathf.Deg2Rad * angle * axis * rotateSpeed; 
        //_rb.MoveRotation(_followTarget.rotation * Quaternion.Euler(rotationOffset));

    }
}
