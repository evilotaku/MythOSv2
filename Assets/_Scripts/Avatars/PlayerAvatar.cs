using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe.AvatarLoader;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerAvatar : NetworkBehaviour
{
    public NetworkVariable<PlayerData> data = new();
    [SerializeField] private Transform PlayerCameraRoot;

    AvatarObjectLoader avatarLoader;
    SkinnedMeshRenderer[] renderers;
    bool isLoaded = false;


    void Awake()
    {
        avatarLoader = new AvatarObjectLoader();
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        data.OnValueChanged += OnDataChange;
        avatarLoader.OnCompleted += OnAvatarLoaded;
        
        
    }

    public override void OnNetworkSpawn()
    {
        if (isLoaded) return;
        print("[Player Avatar] has spawned");
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (string.IsNullOrEmpty(data.Value.AvatarURL)) return;
        Load(data.Value.AvatarURL.ToString());
        isLoaded = true;

        if (!IsLocalPlayer) return;

        print("[Player Avatar] has Set 3rd Person Camera");
        FindObjectOfType<CinemachineVirtualCamera>().Follow = PlayerCameraRoot;
        GetComponent<PlayerInput>().enabled = true;
        GetComponent<CharacterController>().enabled = true;
    }

    void OnDataChange(PlayerData oldValue, PlayerData newValue)
    {
        if (oldValue.AvatarURL == newValue.AvatarURL) return;
        isLoaded = false;
        print("[Player Avatar] Data has changed to " + newValue.AvatarURL.ToString());
        Load(newValue.AvatarURL.ToString());
    }

    public void Load(string _url)
    {
        if (isLoaded) return;
        print("[Player Avatar] Loading Avatar...");
        avatarLoader.LoadAvatar(_url);
    }

    private void OnAvatarLoaded(object sender, CompletionEventArgs args)
    {
        args.Avatar.SetActive(false);
        var newRenderers = args.Avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMesh = newRenderers[i].sharedMesh;
            renderers[i].material = newRenderers[i].material;
            if (!IsOwner)
                renderers[i].gameObject.layer = 0;
        }
        Destroy(args.Avatar);
        isLoaded = true;
    }
}
