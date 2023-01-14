using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardPlayer : MonoBehaviour
{
    public int Id;
    public string DisplayName;
    public bool isAI;
    public bool isActive;

    public Deck DrawDeck;
    public Deck DiscardDeck;
    public List<CardData> Hand;
    public Transform HandLocation;
    public int MaxHandSize, Resources;

    Card cardPrefab;

    public Lane[] lanes = new Lane[5];

    public CardPlayer Opponent;

    public static event Action<Card, int> PlayCard; 

    public void Start()
    {
        Id = Array.IndexOf(CardGameManager.Instance.players, this);
        cardPrefab = CardGameManager.Instance.CardPrefab.GetComponent<Card>();
        ShuffleDeck(DrawDeck);
        Hand = DrawHand(MaxHandSize);
        DisplayHand(Hand,HandLocation);
    }

    public void ShuffleDeck(Deck _deck)
    {
        _deck.Shuffle();
    }
    public void StartTurn()
    {

    }

    List<CardData> DrawHand(int HandSize)
    {
        int numCards = HandSize - Hand.Count;
        if(numCards > DrawDeck.cards.Count)
        {
            numCards = DrawDeck.cards.Count;
        }
        var tmp = new List<CardData>();
        for (int i = 0; i < numCards; i++)
        {
            tmp.Add(DrawDeck.Draw());
        }
        if(DrawDeck.cards.Count == 0) ShuffleDiscardIntoDraw();
        return tmp;
    }

    void DisplayHand(List<CardData> hand, Transform location)
    {        
        int i = 0;
        foreach (var card in location.GetComponentsInChildren<Card>())
        {
            Destroy(card?.gameObject);
        }
        foreach (var card in hand)
        {
            var tmp = Instantiate(cardPrefab,location);
            tmp.data = card;
            tmp.owner = this;
            tmp.transform.Translate(i++ * Vector3.right);
        }
    }

    public async void Play(Card card, int LaneId)
    {
        lanes[LaneId].card = card;        
        Hand.Remove(card.data);
        DisplayHand(Hand, HandLocation);
        card.transform.SetParent(lanes[LaneId].transform, false);
        await card.OnPlay();
        PlayCard.Invoke(card, LaneId);
    }

    public void PitchForResource(Card card)
    {        
        DiscardFromHand(card);
        foreach (var _card in HandLocation.GetComponentsInChildren<Card>())
        {
            if(Resources >= _card.data.Cost)
                _card.state = Card.State.Playable;
        }
    }

    public void DiscardFromHand(Card card)
    {
        DiscardDeck.cards.Insert(0,card.data);
        Hand.Remove(card.data);
        DisplayHand(Hand, HandLocation);
        card.gameObject.transform.SetParent(DiscardDeck.gameObject.transform,false);
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
            if(card.state == Card.State.Dead)
            {
                card.gameObject.transform.SetParent(DiscardDeck.gameObject.transform,false);
            }
        }
    }
}
