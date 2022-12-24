using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class IK_Feet : NetworkBehaviour
{
    public LayerMask terrainLayer;
    public Transform body;
    public IK_Feet otherFoot;

    public float speed = 5f, DistanceTillStep = .1f, stepLength = .1f, stepHeight = .3f;
    public Vector3 footPosOffest, footRotOffset;

    float _footSpacing;
    public float _lerp;
    Vector3 _oldPos, _currPos, _newPos;
    Vector3 _oldNorm, _currNorm, _newNorm;
    bool _init = false;
    Ray ray = new();



    // Start is called before the first frame update
    public void Start()
    {
        _footSpacing = transform.localPosition.x;
        _currPos = _newPos = _oldPos = transform.position;
        _currNorm = _newNorm = _oldNorm = transform.up;
        _lerp = 1f;
        ray.direction = Vector3.down;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _currPos + footPosOffest;
        transform.localRotation = Quaternion.LookRotation(_currNorm) * Quaternion.Euler(footRotOffset);

        ray.origin = (body.position + (body.right * _footSpacing) + (Vector3.up * 2));
        if(Physics.Raycast(ray,out RaycastHit hit, 10, terrainLayer.value))
        {
            if(!_init || Vector3.Distance(_newPos, hit.point) > DistanceTillStep &&  !IsMoving() && !otherFoot.IsMoving())
            {
                _init = true;
                _lerp = 0;
                int direction = body.InverseTransformPoint(hit.point).z > body.TransformPoint(_newPos).z ? 1: -1;
                _newPos = hit.point + (body.forward * (direction * stepLength)); // + footPosOffest;
                _newNorm = hit.normal; // + footRotOffset;
            }

            if(IsMoving())
            {
                Vector3 pos = Vector3.Lerp(_oldPos, _newPos, _lerp);
                pos.y += Mathf.Sin(_lerp * Mathf.PI) * stepHeight;

                _currPos = pos;
                _currNorm = Vector3.Lerp(_oldNorm, _newNorm, _lerp);

                _lerp += Time.deltaTime * speed;
            }else
            {
                _oldPos = _newPos;
                _oldNorm = _newNorm;
            }
        }
    }

    public bool IsMoving()
    {
        return _lerp < 1;
    }
}
