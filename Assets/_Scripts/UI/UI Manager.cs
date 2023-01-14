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
        ActivePlayer.text = CardGameManager.Instance.players[playerIndex].DisplayName;
        PhaseText.text = CardGameManager.currentPhase.ToString();
        await Timer();
    }

    async Task Timer()
    {
        await CardGameManager.Instance.Timer(CardGameManager.Instance.PlayTimer, new Progress<float>(percent =>
        {
            print($"Timer: {percent} ");
            PhaseTimer.value = percent;
        }), CardGameManager.Instance.cancelSource.Token);
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
