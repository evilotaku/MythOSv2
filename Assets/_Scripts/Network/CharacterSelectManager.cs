using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : NetworkBehaviour
{

    public List<GameObject> CharacterPrefabs;
    public int _characterIndex;

    private void Start()
    {        
        NetworkManager.OnClientConnectedCallback += ClientConnection;  
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerPrefabIndex(int index)
    {
        if (index > CharacterPrefabs.Count) return;
        if (NetworkManager.IsListening || IsSpawned) return;

        NetworkManager.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(index);
        _characterIndex = index;
        print($"Selected Character {index + 1}");
    }

    void ClientConnection(ulong clientID)
    {        
        print($"Client {clientID} has connected");
        SpawnPlayerObjectServerRPC(_characterIndex);
    }

    [ServerRpc]
    void SpawnPlayerObjectServerRPC(int index, ServerRpcParams param = default)
    {
        print($"Spawning Character {_characterIndex} for Player {param.Receive.SenderClientId}");
        Instantiate(CharacterPrefabs[_characterIndex])
                .GetComponent<NetworkObject>()
                .SpawnAsPlayerObject(param.Receive.SenderClientId);
    }

    void Update()
    {

    }

}
