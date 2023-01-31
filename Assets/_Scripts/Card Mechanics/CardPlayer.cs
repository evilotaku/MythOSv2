using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class CardPlayer : NetworkBehaviour
{
    public int Id;
    public string DisplayName;
    public bool isAI;
    public bool isActive;

    public PlayerUI PlayerUI;
    public Deck DrawDeck;
    public Deck DiscardDeck;
    public List<int> Hand;
    public Transform HandLocation;
    public int MaxHandSize, Resources;

    Card cardPrefab;
    CardDB CardDB;

    public Lane[] lanes = new Lane[5];

    //public CardPlayer Opponent;

    public static event Action<int, int> PlayCard;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Id = Array.IndexOf(CardGameManager.Instance.players, this);      
        CardDB = CardGameManager.Instance.CardsDB;
        cardPrefab = CardDB.CardPrefab.GetComponent<Card>();
    }

    public void ShuffleDeck(Deck _deck)
    {
        _deck.ShuffleServerRpc();
    }
    public void StartTurn()
    {

    }

    List<int> DrawHand(int HandSize)
    {
        int numCards = HandSize - Hand.Count;
        if(numCards > DrawDeck.cards.Count)
        {
            numCards = DrawDeck.cards.Count;
        }
        var tmp = new List<int>();
        for (int i = 0; i < numCards; i++)
        {
            tmp.Add(DrawDeck.Draw());
        }
        if(DrawDeck.cards.Count == 0) ShuffleDiscardIntoDraw();
        return tmp;
    }

    [ClientRpc]
    void DisplayHandClientRpc(int[] hand)
    {        
        int i = 0;
        foreach (var card in HandLocation.GetComponentsInChildren<Card>())
        {
            Destroy(card?.gameObject);
        }
        foreach (var card in hand)
        {
            var tmp = Instantiate(cardPrefab, HandLocation);
            tmp.data = CardDB.CardByID(card);
            tmp.owner = this;
            tmp.transform.Translate(i++ * Vector3.right);
        }
    }

    [ServerRpc]
    public void PlayServerRpc(int cardId, int LaneId)
    {
        Play(cardId, LaneId);
    }

    public async void Play(int cardId, int LaneId)
    {
        lanes[LaneId].card = CardDB.CardByID(cardId);         
        Hand.Remove(cardId);
        DisplayHandClientRpc(Hand.ToArray());
        //card.transform.SetParent(lanes[LaneId].transform, false);
        await lanes[LaneId].card.OnPlay();
        PlayCard.Invoke(cardId, LaneId);
    }

    public void PitchForResource(int cardId)
    {        
        DiscardFromHand(cardId);
        foreach (var _card in HandLocation.GetComponentsInChildren<Card>())
        {
            if(Resources >= _card.data.Cost)
                _card.data.state = CardData.State.Playable;
        }
    }

    public void DiscardFromHand(int cardId)
    {
        DiscardDeck.NetDeck.Insert(0, cardId);
        Hand.Remove(cardId);
        DisplayHandClientRpc(Hand.ToArray());
        //card.gameObject.transform.SetParent(DiscardDeck.gameObject.transform,false);
        //or Destroy(card.gameobject);
    }

    public void ShuffleDiscardIntoDraw()
    {
        DrawDeck = DiscardDeck;
        ShuffleDeck(DrawDeck);
    }

    public void UpdateCards()
    {
        foreach (var lane in lanes)
        {
            var card = lane.GetComponent<Card>();
            card.UpdateCard();
            if(card.data.state == CardData.State.Dead)
            {
                card.gameObject.transform.SetParent(DiscardDeck.gameObject.transform,false);
            }
        }
    }
}
