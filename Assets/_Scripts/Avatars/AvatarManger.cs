using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Text;
using Unity.Collections;
using System;
using Newtonsoft.Json;

[Serializable]
public struct PlayerData : INetworkSerializable,IEquatable<PlayerData>
{
    public ulong ClientId;
    public string AvatarURL;
    public int Gender;

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId && 
               AvatarURL == other.AvatarURL &&
               Gender == other.Gender;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref AvatarURL);
        serializer.SerializeValue(ref Gender);
    }
}
public enum Gender
{
    Male = 0,
    Female = 1
}
public class AvatarManger : NetworkBehaviour
{    
    public string AvatarURL;
    public Gender Gender;
    [NonReorderable]
    public GameObject[] AvatarBase;
    //public NetworkList<PlayerData> AvatarList;
    List<PlayerData> PlayerList = new();
    PlayerData _localPlayerData, _remotePlayerData;

    public bool Test;

    void Awake()
    {
        //AvatarList = new NetworkList<PlayerData>();
    }    

    // Start is called before the first frame update
    void Start()
    {
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        var data = _localPlayerData = new PlayerData
                                    {
                                        AvatarURL = this.AvatarURL,
                                        Gender = (int)this.Gender
                                    };
        var dataJson = JsonConvert.SerializeObject(data);
        print($"Serializing {dataJson}");
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(dataJson);

        if (Test) NetworkManager.Singleton.StartHost();

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        print($"Avatar Manager is spawned...");        
        /*if (IsClient) AvatarList.OnListChanged += AvatarListChanged;
        if(IsServer)
        {
            AvatarList.OnListChanged += AvatarListChanged;            
        }*/
    }

    void OnGetAvatarURL(string url)
    {
        AvatarURL = url;
    }

    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        NetworkObject.Spawn();        
        var json = Encoding.ASCII.GetString(request.Payload);
        print($"recieved: {json}");
        PlayerData newPlayer = JsonConvert.DeserializeObject<PlayerData>(json);        
        newPlayer.ClientId = request.ClientNetworkId;
        PlayerList.Add(newPlayer);
        //AvatarList.Add(newPlayer);
        _remotePlayerData = newPlayer;

        response.CreatePlayerObject = false;
        response.PlayerPrefabHash = null;
        response.Approved = true;
        response.Pending = false;
        //callback(createPlayerObject: false, playerPrefabHash: null,approved: true, position:Vector3.zero, rotation: Quaternion.identity);
    }

    /*void AvatarListChanged(NetworkListEvent<PlayerData> evt)
    {
        print("Avatar List has Changed... " + evt.Type.ToString());
        
        Action x = evt.Type switch
        {       
            NetworkListEvent<PlayerData>.EventType.Add =>()=>
            {
                if(!IsServer || !IsHost) return;
                print("New Player added to List...");
                var pos = UnityEngine.Random.insideUnitCircle;
                var newPlayer = Instantiate(AvatarBase[(int)Gender], new Vector3(pos.x, 0, pos.y), Quaternion.LookRotation(-pos));
                newPlayer.GetComponent<PlayerAvatar>().data.Value = evt.Value;
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(evt.Value.ClientId);
                return;
            },
            NetworkListEvent<PlayerData>.EventType.Full =>() =>
            {
                print("Full List Update..");
                LoadAvatars();
            },
            _ => null
        };
        x.Invoke();
        
    }*/

    
    void ReLoadAvatars()
    {
        print("Reloading all Avatars...");
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
        if (NetworkManager.Singleton.IsHost)
        {
            var pos = UnityEngine.Random.insideUnitCircle;
            var newPlayer = Instantiate(AvatarBase[(int)Gender], new Vector3(pos.x, 0, pos.y), Quaternion.LookRotation(-new Vector3(pos.x, 0, pos.y)));
            newPlayer.GetComponent<PlayerAvatar>().data.Value = PlayerList[(int)_clientID];
            newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(_clientID);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


