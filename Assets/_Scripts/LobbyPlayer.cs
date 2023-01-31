using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections.LowLevel.Unsafe;

public class LobbyPlayer : MonoBehaviour
{

    public string DisplayName;

    public List<CardData> Deck;

    public static event Action<string, List<int>> LoadPlayer;

    // Start is called before the first frame update
    void Start()
    {
        LoadPlayer.Invoke(DisplayName, LoadDeck());
    }

    public List<int> LoadDeck()
    {
        List<int> temp = new();
        foreach (var card in Deck)
        {
            temp.Add(card.ID);
        }
        return (temp.Count == 0) ? null : temp;
    }

}
