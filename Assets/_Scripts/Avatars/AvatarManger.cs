using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Text;
using System;
using Newtonsoft.Json;
using ReadyPlayerMe.AvatarLoader;
using Newtonsoft.Json.Serialization;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public string DisplayName;
    public string AvatarURL;
    public OutfitGender Gender;
    public bool IsVR;
    public int[] Deck;

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId &&
               DisplayName == other.DisplayName && 
               AvatarURL == other.AvatarURL &&
               Gender == other.Gender &&
               IsVR == other.IsVR &&
               Deck == other.Deck;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;
        if (!serializer.IsReader) length = Deck.Length;
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref DisplayName);
        serializer.SerializeValue(ref AvatarURL);
        serializer.SerializeValue(ref Gender);
        serializer.SerializeValue(ref IsVR);
        serializer.SerializeValue(ref length);
        if (serializer.IsReader) Deck = new int[length];
        for (int i = 0; i < length; ++i)
        {
            serializer.SerializeValue(ref Deck[i]);
        }
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

    AvatarObjectLoader loader;

    private void Awake()
    {               
        loader = new();
        loader.OnCompleted += Loader_OnCompleted;
        LobbyPlayer.LoadPlayer += OnPlayerLoaded;
    }

    void Start()
    {
        print("[AvatarManager] Started...");
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnection;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void OnPlayerLoaded(string name, List<int> _netdeck)
    {
        print($"Deck loaded...{string.Join(", ", _netdeck)}");
        LocalPlayer.Deck = _netdeck.ToArray();
        LocalPlayer.DisplayName = name;
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
        if (IsServer) return;
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