using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName ="UI/PlayerUI")]
public class PlayerUI : ScriptableObject
{
    public string DisplayName;
    public string PhaseName;
    public float PlayTimer;

    public CardData[] Hand;
    public CardData[] Lanes;
    public CancellationTokenSource cancelSource = new();

    public async Task Timer(float amount, IProgress<float> progress, CancellationToken cancelToken)
    {
        for (int i = 0; i <= amount; i++)
        {
            if (cancelToken.IsCancellationRequested) break;
            await Task.Delay(1000);
            progress?.Report(i / amount);
        }
        Debug.Log("Times Up!");
    }

}
