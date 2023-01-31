using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Lane : MonoBehaviour, IDropHandler
{
    public CardData card;

    public void OnDrop(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
