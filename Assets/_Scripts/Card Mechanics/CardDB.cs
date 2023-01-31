using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(menuName = "Cards/CardDB")]
public class CardDB : ScriptableObject
{
    [SerializeField]
    public CardData[] CardPool;
    public GameObject CardPrefab;

    

    public void GetAllCards() => CardPool = Resources.LoadAll<CardData>("CardData");

    public CardData CardByID(int Id)
    {
        foreach (var card in CardPool)
        {
            if (card.ID == Id)
            {
                return card;
            }
        }
        return null;
    }
}
