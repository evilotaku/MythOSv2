using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<CardData> cards;

    public void Add(CardData card)
    {
        cards.Add(card);
    }

    public CardData Draw()
    {
        CardData tmp = cards[0];
        cards.Remove(tmp);
        return tmp;
    }

    public void Shuffle()
    {
        int n = cards.Count;  
        while (n > 1) 
        {  
            n--;  
            int k = Random.Range(0,n + 1);  
            CardData value = cards[k];  
            cards[k] = cards[n];  
            cards[n] = value;  
        }  
    }
    
}
