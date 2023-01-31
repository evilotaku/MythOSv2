using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI PhaseText, ActivePlayer;
    public Slider PhaseTimer;
    public PlayerUI PlayerUI;
    public CardDB CardDB;

    void OnEnable()
    {
        CardGameManager.OnUpkeep += UpdateUI;
        CardGameManager.OnPlay += UpdateUI;
        CardGameManager.OnCombat += UpdateUI;
        CardGameManager.OnDiscard += UpdateUI;
        CardGameManager.OnDraw += UpdateUI;
    }

    async void UpdateUI(int playerIndex)
    {
        ActivePlayer.text = PlayerUI.DisplayName;
        PhaseText.text = PlayerUI.PhaseName;
        await Timer();
    }

    async Task Timer()
    {
        await PlayerUI.Timer(PlayerUI.PlayTimer, new Progress<float>(percent =>
        {
            print($"Timer: {percent} ");
            PhaseTimer.value = percent;
        }), PlayerUI.cancelSource.Token);
    }

    void OnDisable()
    {
        CardGameManager.OnUpkeep -= UpdateUI;
        CardGameManager.OnPlay -= UpdateUI;
        CardGameManager.OnCombat -= UpdateUI;
        CardGameManager.OnDiscard -= UpdateUI;
        CardGameManager.OnDraw -= UpdateUI;
    }
}
