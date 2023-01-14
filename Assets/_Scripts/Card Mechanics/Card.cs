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
    [Flags]
    public enum State
    {
        Playable = 1,
        Selected = 2,
        Played = 4,
        Pitched = 8,
        Ready = 16,
        Used = 32,
        Damaged = 64,
        Dead = 128

    }
    public CardPlayer owner;
    public CardData data;

    public State state;

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
        UpdateCard();
    }

    public void UpdateCard()
    {
        Cost.text = CurrentCost.ToString();
        Life.text = CurrentLife.ToString();
        Power.text = CurrentPower.ToString();  
        if(CurrentLife <= 0) OnDeath(); 
    }


    public async Task OnPlay()
    {
        state = State.Played; 
        
        foreach (var effect in data.PlayEffects)
        {
            await effect.ApplyEffect();
        }       
        
        owner.UpdateCards();
    }

    public async Task Use()
    {
        if (state.HasFlag(State.Used))
        {
            //Play Animation/Sound
            return; 
        }
        state |= State.Used;        
        transform.Rotate(new Vector3(0,90,0));        

        foreach (var effect in data.UseEffects)
        {
            await effect.ApplyEffect();
        }       
        
        owner.UpdateCards();
        
    }

    public async Task OnPitch()
    {
        state = State.Pitched;         
        foreach (var effect in data.PitchEffects)
        {
            await effect.ApplyEffect();
        }       
        owner.Resources += CurrentCost;
    }

    public async Task OnUpkeep()
    {         
        if(state.HasFlag(~State.Played)) return;
        state ^= State.Used;
        state |= State.Ready; 
        foreach (var effect in data.UpkeepEffects)
        {
            await effect.ApplyEffect();
        }   
        owner.UpdateCards(); 
    }

    public async Task OnCombat()
    {
        if(state.HasFlag(~State.Played|State.Used|State.Dead)) return;
        foreach (var effect in data.CombatEffects)
        {
            await effect.ApplyEffect();
        }
        owner.UpdateCards();        
    }

    public async Task OnDeath()
    {
        state = State.Dead;
        foreach (var effect in data.DeathEffects)
        {
            await effect.ApplyEffect();
        }
        owner.UpdateCards();
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
