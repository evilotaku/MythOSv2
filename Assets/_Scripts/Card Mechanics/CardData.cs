using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

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
