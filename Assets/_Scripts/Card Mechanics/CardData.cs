using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public struct NetCard : INetworkSerializable
{
    public int ID;
    public int Quantity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref Quantity);
    }
}

[CreateAssetMenu(menuName = "Cards")]
public class CardData : ScriptableObject
{
    public int ID;
    public string Name;
    public int Cost;
    public int MaxPower;
    public int MaxLife;
    public Sprite Image;
    public List<CardEffect> PlayEffects;
    public List<CardEffect> PitchEffects;
    public List<CardEffect> UpkeepEffects;
    public List<CardEffect> UseEffects;
    public List<CardEffect> CombatEffects;
    public List<CardEffect> DeathEffects;

    private void OnValidate()
    {
        if (ID == 0) ID = Guid.NewGuid().GetHashCode();
    }
}
