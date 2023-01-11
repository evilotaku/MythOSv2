using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class NetworkThirdPersonCameraSetup : NetworkBehaviour
{
    public Transform PlayerCameraRoot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        FindObjectOfType<CinemachineVirtualCamera>().Follow = PlayerCameraRoot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
