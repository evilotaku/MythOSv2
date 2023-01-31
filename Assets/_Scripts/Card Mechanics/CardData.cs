using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


[Serializable]
public struct NetCard : INetworkSerializable, IEquatable<NetCard>
{


    public int ID;
    public int Quantity;
    

    public bool Equals(NetCard other) => Equals(other);

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref Quantity);
    }
}

[CreateAssetMenu(menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    [Flags]
    public enum State
    {
        Playable = 1,
        Selected = 2,
        Played = 4,
        Pitched = 8,
        Ready = 16,
        Used = 32,
        Damaged = 64,
        Dead = 128

    }
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
    public State state;
    public Action UpdateCard;
    public Action<int> UpdateResources;

    private void OnValidate()
    {
        ID = GetHashCode();
    }

    public async Task OnPlay()
    {
        state = State.Played;

        foreach (var effect in PlayEffects)
        {
            await effect.ApplyEffect();
        }

        UpdateResources.Invoke(-Cost);
        UpdateCard.Invoke();
    }

    public async Task Use()
    {
        if (state.HasFlag(State.Used))
        {
            //Play Animation/Sound
            return;
        }
        state |= State.Used;        

        foreach (var effect in UseEffects)
        {
            await effect.ApplyEffect();
        }

        UpdateCard.Invoke();

    }

    public async Task OnPitch()
    {
        state = State.Pitched;
        foreach (var effect in PitchEffects)
        {
            await effect.ApplyEffect();
        }
        UpdateResources.Invoke(Cost);
    }

    public async Task OnUpkeep()
    {
        if (state.HasFlag(~State.Played)) return;
        state ^= State.Used;
        state |= State.Ready;
        foreach (var effect in UpkeepEffects)
        {
            await effect.ApplyEffect();
        }
        UpdateCard.Invoke();
    }

    public async Task OnCombat()
    {
        if (state.HasFlag(~State.Played | State.Used | State.Dead)) return;
        foreach (var effect in CombatEffects)
        {
            await effect.ApplyEffect();
        }
        UpdateCard.Invoke();
    }

    public async Task OnDeath()
    {
        state = State.Dead;
        foreach (var effect in DeathEffects)
        {
            await effect.ApplyEffect();
        }
        UpdateCard.Invoke();
    }
}
