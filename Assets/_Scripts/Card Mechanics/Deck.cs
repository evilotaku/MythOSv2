using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Deck : NetworkBehaviour
{
    public List<CardData> cards;
    public List<int> NetDeck;
    public CardDB CardDB;


    private void Awake()
    {
        CardDB = (CardDB)Resources.Load("Scriptable Objects", typeof(CardDB));
    }
    public List<int> LoadDeck()
    {
        NetDeck = new();
        foreach (var card in cards)
        {
            NetDeck.Add(card.ID);
        }
        return (NetDeck.Count == 0) ? null : NetDeck;
    }

    public List<CardData> IntToDeck()
    {
        cards = new();
        foreach (var id in NetDeck)
        {
            cards.Add(CardDB.CardByID(id));
        }
        return (cards.Count == 0) ? null : cards;
    }
    public void Add(int card)
    {
        NetDeck.Add(card);
    }

    public int Draw()
    {
        int tmp = NetDeck[0];
        NetDeck.Remove(tmp);
        return tmp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShuffleServerRpc()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        int n = NetDeck.Count;  
        while (n > 1) 
        {  
            n--;  
            int k = UnityEngine.Random.Range(0,n + 1);  
            int value = NetDeck[k];
            NetDeck[k] = NetDeck[n];
            NetDeck[n] = value;  
        }  
    }

    

}
