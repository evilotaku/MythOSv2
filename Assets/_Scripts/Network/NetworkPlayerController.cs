using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using Unity.XR;
using TMPro;

public class NetworkPlayerController : NetworkBehaviour
{
    public XROrigin XR_rig;
    public Head head;
    public Hand lHand, rHand;
    public static event Action OnPlayerSpawn;
    
    
    public override void OnNetworkSpawn()
    {
        if(!enabled) return;
        base.OnNetworkSpawn();


        print("Invoking OnPlayerSpawn...");
        OnPlayerSpawn?.Invoke();
        if (IsLocalPlayer)
        {
            print("Local Player Spawned...");
            //lHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            //rHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            XR_rig = FindObjectOfType<XROrigin>();
            head.followObject = XR_rig.Camera.transform;
            lHand.FollowTarget = FindObjectOfType<LeftHandTag>().transform;
            rHand.FollowTarget = FindObjectOfType<RightHandTag>().transform; ;
            XR_rig.transform.position = transform.position;
            XR_rig.transform.rotation = transform.rotation;
            //head.GetComponent<Head>().followObject = XR_rig.Camera.transform;      
        }
        else
        {            
            head.enabled = false;
            lHand.enabled = false;
            rHand.enabled = false;
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
