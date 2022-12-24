using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Vivox;
using VivoxUnity;
using ParrelSync;
using Unity.Networking.Transport.Relay;

public class NetworkConnectionManager : NetworkBehaviour
{
    public bool VoiceChat, DebugHost;
    int MaxConnections = 100;
    string RelayCode;
    RelayAllocationId AllocationID;
    Lobby lobby;
    ILoginSession LoginSession;


    // Start is called before the first frame update
    async void Start()
    {
        var options = new InitializationOptions();
#if UNITY_EDITOR
        if (ClonesManager.IsClone())
        {
            Debug.Log("This is a clone project.");
            string customArgument = ClonesManager.GetArgument();
            options.SetProfile(customArgument);
        }
#endif

        
        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        print($"Logged in as {AuthenticationService.Instance.PlayerId} ");
        VivoxService.Instance.Initialize();


    }


    [ContextMenu("Join or Create Lobby")]
    async public void JoinOrCreateLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new();
            options.Filter = new List<QueryFilter>()
            {
                new QueryFilter(field: QueryFilter.FieldOptions.MaxPlayers,
                                value: "10",
                                op: QueryFilter.OpOptions.GE
                                )
            };

            options.Player = new Player();

            lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);
            print($"Joined Lobby: {lobby.Name}");
            RelayCode = lobby.Data["RelayCode"].Value;            
            StartClient();

        }
        catch
        {
            print("No Lobbies Found. Creating New Lobby");
            StartCoroutine(StartHost());
        }

        if(VoiceChat)
        {
            print("Logging into Vivox Voice Chat...");
            VivoxLogin();
        }


    }

    async public void StartClient()
    {
        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(RelayCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            await Task.Yield();
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            await Task.FromException(clientRelayUtilityTask.Exception);
        }

        var relayData = clientRelayUtilityTask.Result;

        var options = new UpdatePlayerOptions
        {
            AllocationId = clientRelayUtilityTask.Result.AllocationId.ToString()
        };        
        await Lobbies.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, options);
        
        // When connecting as a client to a Relay server, connectionData and hostConnectionData are different.
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);

        print($"Starting Client on {relayData.Endpoint.Address} : {relayData.Endpoint.Port}");
        NetworkManager.Singleton.StartClient();
        await Task.Yield();
    }

    public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }
        
        Debug.Log($"client: Connection Data[0] {allocation.ConnectionData[0]} Connection Data[1] {allocation.ConnectionData[1]}");
        Debug.Log($"host: Connection Data[0] {allocation.HostConnectionData[0]} Connection Data[1] {allocation.HostConnectionData[1]}");
        Debug.Log($"client: Allocation ID {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }


    public IEnumerator StartHost()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var (relayData, joinCode) = serverRelayUtilityTask.Result;
        // Display the joinCode to the user.
        print($"joinCode: {joinCode}");

        var task = CreateLobby(joinCode, relayData.AllocationId);

        // When starting a Relay server, both instances of connection data are identical.
        print($"Starting Server on {relayData.Endpoint.Address} : {relayData.Endpoint.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayData);
        NetworkManager.Singleton.StartHost();
        yield return null;
    }

    public static async Task<(RelayServerData, string)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        string createJoinCode;
        try
        {
            allocation = await Relay.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server Connection Data: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server Allocation ID: {allocation.AllocationId}");

        try
        {
            createJoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }


        return (new RelayServerData(allocation, "dtls"), createJoinCode);
    }
    async Task CreateLobby(string relayCode, RelayAllocationId allocationId)
    {
        try
        {
            print("Creating Lobby...");
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                        "RelayCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: relayCode,
                        index: DataObject.IndexOptions.S1
                    )
                },
                {
                        "Relay Allocation", new DataObject(
                        DataObject.VisibilityOptions.Member,
                        AllocationID.ToString(),
                        DataObject.IndexOptions.S2
                    )
                }
            };
            lobby = await Lobbies.Instance.CreateLobbyAsync("Main Lobby", MaxConnections, options);

            var playerOptions = new UpdatePlayerOptions();
            playerOptions.AllocationId = allocationId.ToString();
            await Lobbies.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, playerOptions);

            print($"Lobby {lobby.Id} is created with Lobby code {lobby.LobbyCode}");
            StartCoroutine(HearbeatLobby(lobby.Id, 15));
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    IEnumerator HearbeatLobby(string lobbyId, float waitTimeinSecs)
    {
        var delay = new WaitForSecondsRealtime(waitTimeinSecs);
        while (true)
        {
            print("Lobby hearbeat...");
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }

    }

    public void JoinVivoxChannel(string channelName, ChannelType channelType, bool connectAudio, bool connectText, bool transmissionSwitch = true, Channel3DProperties properties = null)
    {
        if (LoginSession.State == LoginState.LoggedIn)
        {
            Channel channel = new Channel(channelName, channelType, properties);

            IChannelSession channelSession = LoginSession.GetChannelSession(channel);

            channelSession.BeginConnect(connectAudio, connectText, transmissionSwitch, channelSession.GetConnectToken(), ar =>
            {
                try
                {
                    channelSession.EndConnect(ar);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not connect to channel: {e.Message}");
                    return;
                }
            });
        }
        else
        {
            Debug.LogError("Can't join a channel when not logged in.");
        }
    }

    public void VivoxLogin(string displayName = null)
    {
        var account = new Account(displayName);
        bool connectAudio = true;
        bool connectText = true;

        LoginSession = VivoxService.Instance.Client.GetLoginSession(account);
        LoginSession.PropertyChanged += LoginSession_PropertyChanged;

        LoginSession.BeginLogin(LoginSession.GetLoginToken(), SubscriptionMode.Accept, null, null, null, ar =>
        {
            try
            {
                LoginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                // Unbind any login session-related events you might be subscribed to.
                // Handle error
                return;
            }
            // At this point, we have successfully requested to login.�
            // When you are able to join channels, LoginSession.State will be set to LoginState.LoggedIn.
            // Reference LoginSession_PropertyChanged()
        });
    }

    // For this example, we immediately join a channel after LoginState changes to LoginState.LoggedIn.
    // In an actual game, when to join a channel will vary by implementation.
    private void LoginSession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var loginSession = (ILoginSession)sender;
        if (e.PropertyName == "State")
        {
            if (loginSession.State == LoginState.LoggedIn)
            {
                bool connectAudio = true;
                bool connectText = true;

                // This puts you into an echo channel where you can hear yourself speaking.
                // If you can hear yourself, then everything is working and you are ready to integrate Vivox into your project.
                //JoinVivoxChannel("TestChannel", ChannelType.Echo, connectAudio, connectText);
                // To test with multiple users, try joining a non-positional channel.
                JoinVivoxChannel("MultipleUserTestChannel", ChannelType.NonPositional, connectAudio, connectText);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LogOff()
    {
        LoginSession?.Logout();
        VivoxService.Instance.Client.Uninitialize();        
        if (lobby == null) return;
        if(NetworkManager.Singleton.IsHost)
            Lobbies.Instance.DeleteLobbyAsync(lobby.Id);
        NetworkManager.Singleton.Shutdown();
    }

    void OnApplicationQuit()
    {
        LogOff();
    }

}