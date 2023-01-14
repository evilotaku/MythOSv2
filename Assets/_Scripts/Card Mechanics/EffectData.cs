using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct EffectData 
{
    public EffectType Type;
    public int amount;

    //TODO: Add Effected Player

    public List<Card> Targets;
}

public enum EffectType
{
    DrawCard,
    Power,
    Life
}
