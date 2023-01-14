using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[System.Serializable]
public struct CardEffect
{
    public string Name;
    public EffectData data;

    public static event Action<EffectData> EffectEvt;

    public async Task ApplyEffect()
    {
        await Task.Delay(1000);
        EffectEvt?.Invoke(data);        
    }
}
