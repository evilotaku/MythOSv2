using System;
using Unity.Netcode;
using Unity.XR.CoreUtils;

public class NetworkPlayerController : NetworkBehaviour
{
    public XROrigin XR_rig;
    public Head head;
    public Hand lHand, rHand;
    public static event Action OnPlayerSpawn;

    public override void OnNetworkSpawn()
    {
        if(!enabled) return;

        print("Invoking OnPlayerSpawn...");
        OnPlayerSpawn?.Invoke();
        if (IsLocalPlayer)
        {
            print("Local Player Spawned...");
            XR_rig = FindObjectOfType<XROrigin>();
            head.followObject = XR_rig.Camera.transform;
            lHand.FollowTarget = FindObjectOfType<LeftHandTag>().transform;
            rHand.FollowTarget = FindObjectOfType<RightHandTag>().transform; ;
            XR_rig.transform.position = transform.position;
            XR_rig.transform.rotation = transform.rotation;
        }
        else
        {            
            head.enabled = false;
            lHand.enabled = false;
            rHand.enabled = false;
        }
    }
}
