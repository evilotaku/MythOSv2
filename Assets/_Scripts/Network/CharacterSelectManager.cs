using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;

public class CharacterSelectManager : NetworkBehaviour
{

    public List<GameObject> CharacterPrefabs;
    int _characterIndex;
    bool _loaded = false;


    public override void OnNetworkSpawn()
    {        
        if (_loaded) return;               
        SpawnPlayerObjectServerRPC(_characterIndex);
        _loaded = true;
    }

    public void SetPlayerPrefabIndex(int index) => _characterIndex = index;


    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerObjectServerRPC(int index, ServerRpcParams param = default)
    {
        var pos = UnityEngine.Random.insideUnitCircle;
        Instantiate(CharacterPrefabs[index],
            new Vector3(pos.x, 0, pos.y),
            Quaternion.LookRotation(-new Vector3(pos.x, 0, pos.y)))
                .GetComponent<NetworkObject>()
                .SpawnAsPlayerObject(param.Receive.SenderClientId);
    }

    void Update()
    {

    }
   

}
