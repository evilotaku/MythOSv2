using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Text;
using Unity.Collections;
using System;
using Newtonsoft.Json;
using Unity.Netcode.Transports.UTP;
using ReadyPlayerMe;

[Serializable]
public struct PlayerData : INetworkSerializable,IEquatable<PlayerData>
{
    public ulong ClientId;
    public string AvatarURL;
    public OutfitGender Gender;
    public bool IsVR;

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId && 
               AvatarURL == other.AvatarURL &&
               Gender == other.Gender &&
               IsVR == other.IsVR;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref AvatarURL);
        serializer.SerializeValue(ref Gender);
        serializer.SerializeValue(ref IsVR);
    }
}
public class AvatarManger : NetworkBehaviour
{
    [SerializeField] private PlayerData LocalPlayer;
    [NonReorderable]
    [SerializeField] private GameObject[] VRAvatarBase;
    [NonReorderable]
    [SerializeField] private GameObject[] ThirdPersonAvatarBase;
    List<PlayerData> PlayerList = new();

    [SerializeField] private bool Test;

    AvatarLoader loader;

   
    void Start()
    {
        loader = new();
        loader.OnCompleted += Loader_OnCompleted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        loader.LoadAvatar(LocalPlayer.AvatarURL);        

    }

    private void Loader_OnCompleted(object sender, CompletionEventArgs e)
    {
        Destroy(e.Avatar);
        LocalPlayer.Gender = e.Metadata.OutfitGender;

        var dataJson = JsonConvert.SerializeObject(LocalPlayer);
        print($"[Avatar Manager] is Serializing {dataJson}");
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(dataJson);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        print($"[Avatar Manager] is spawned...");   
    }

    void OnGetAvatarURL(string url)
    {
        LocalPlayer.AvatarURL = url;
    }

    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        print("[Avatar Manager] Connection Approval...");
        //NetworkObject.Spawn();        
        var json = Encoding.ASCII.GetString(request.Payload);
        print($"[Avatar Manager] recieved: {json}");
        PlayerData newPlayer = JsonConvert.DeserializeObject<PlayerData>(json);        
        newPlayer.ClientId = request.ClientNetworkId;
        PlayerList.Add(newPlayer);

        response.CreatePlayerObject = false;
        response.PlayerPrefabHash = null;
        response.Approved = true;
        response.Pending = false;
    }
    
    void ReLoadAvatars()
    {
        print("[Avatar Manager] is Reloading all Avatars...");
        if(IsServer) return;
        foreach (var player in PlayerList)
        {
            NetworkManager.Singleton.ConnectedClients[player.ClientId]
                            .PlayerObject.GetComponent<PlayerAvatar>()
                            .data.Value = player;
        }
    }
    void OnConnection(ulong _clientID)
    {   
        if (NetworkManager.Singleton.IsServer)
        {
            var player = PlayerList[(int)_clientID];
            var prefab = player.IsVR ? VRAvatarBase[(int)player.Gender - 1] : ThirdPersonAvatarBase[(int)player.Gender - 1];
            var pos = UnityEngine.Random.insideUnitCircle; 
            var newPlayer = Instantiate(prefab, new Vector3(pos.x, 0, pos.y), Quaternion.LookRotation(-new Vector3(pos.x, 0, pos.y)));
            newPlayer.GetComponent<PlayerAvatar>().data.Value = player;
            print("[Avatar Manager] is Spawning Player Avatar...");
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(_clientID);
        }
    }    
}