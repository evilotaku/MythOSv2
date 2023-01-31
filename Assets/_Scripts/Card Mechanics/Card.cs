using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
   
    public CardPlayer owner;
    public CardData data;

   

    public int CurrentCost, CurrentPower, CurrentLife;

    public TMP_Text Name, Cost, Power, Life;

    public SpriteRenderer SpriteRender;
    //public bool Playable, InPlay, Exhausted;


    private void Start()
    {
       // SpriteRender = GetComponentInChildren<SpriteRenderer>();
        Name.text = data.Name;        
        CurrentCost = data.Cost;        
        CurrentLife = data.MaxLife;        
        CurrentPower = data.MaxPower;
        SpriteRender.sprite = data.Image;
        data.UpdateCard += UpdateCard;
    }


    public void UpdateCard()
    {
        Cost.text = CurrentCost.ToString();
        Life.text = CurrentLife.ToString();
        Power.text = CurrentPower.ToString();
        if (data.state.HasFlag(~CardData.State.Used))
            transform.Rotate(new Vector3(0, 90, 0));
        if (CurrentLife <= 0) data.OnDeath();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }


    //Add Purchace
    //Advance Time to Live
}
