using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe;
using Unity.Netcode;

public class PlayerAvatar : NetworkBehaviour
{
    public string AvatarURL;
    public Gender Gender;
    public NetworkVariable<PlayerData> data = new NetworkVariable<PlayerData>();
    AvatarLoader avatarLoader;

    SkinnedMeshRenderer[] renderers;
    public bool isLoaded = false;

    void Awake()
    {
        avatarLoader = new AvatarLoader();
        data.OnValueChanged += OnDataChange;
        avatarLoader.OnCompleted += OnAvatarLoaded;
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if(string.IsNullOrEmpty(AvatarURL)) return; 
        Load(AvatarURL); 
    }
    public override void OnNetworkSpawn()
    {
        print("OnNetworkSpawn...");
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if(string.IsNullOrEmpty(AvatarURL)) return; 
        Load(data.Value.AvatarURL.ToString()); 
    }

    void OnDataChange(PlayerData oldValue, PlayerData newValue)
    {
        print("OnDataChange to new value " + newValue.AvatarURL.ToString());
        var url = newValue.AvatarURL.ToString();
        if(string.IsNullOrEmpty(url)) return; 
        AvatarURL = url; 
        Load(AvatarURL);   
    }

    public void Load(string _url)
    {        
        
        if(isLoaded) return; 
        print("Loading...");
        avatarLoader.LoadAvatar(_url); //, (avatar)=>{avatar.SetActive(false);}, OnAvatarLoaded);
    }

    private void OnAvatarLoaded(object _, CompletionEventArgs args)
    {
        args.Avatar.SetActive(false);
        var newRenderers = args.Avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMesh = newRenderers[i].sharedMesh;
            renderers[i].material = newRenderers[i].material;
        }
        Destroy(args.Avatar);
        isLoaded = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
