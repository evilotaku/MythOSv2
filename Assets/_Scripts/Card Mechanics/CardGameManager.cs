using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;

public enum TurnPhase
{
    EndTurn,
    Upkeep,
    Play,
    Combat,
    Discard,
    Draw
}
public class CardGameManager : NetworkBehaviour
{
    public static event Action<int> OnUpkeep, OnPlay, OnCombat, OnDiscard, OnDraw, OnEndTurn;
    public static event Action<float> StartPlayTimer, StartDiscardTimer;
    public static CardGameManager Instance;
    public GameObject CardPrefab;
    public float PlayTimer;
    public CancellationTokenSource cancelSource = new();
    public static TurnPhase currentPhase = 0;
    public CardPlayer[] players;
    public int ActivePlayerID;

    bool FirstTurn = true;

    void Awake()
    {
        if(!Instance) Instance = this;
    }

    void OnEnable()
    {
        OnUpkeep += UpkeepPhase;
        OnPlay += PlayPhase;
        OnCombat += CombatPhase;
        OnDiscard += DiscardPhase;
        OnDraw += DrawPhase;
        print("Actions subbed...");
    }   

    void OnDisable()
    {
        OnUpkeep -= UpkeepPhase;
        OnPlay -= PlayPhase;
        OnCombat -= CombatPhase;
        OnDiscard -= DiscardPhase;
        OnDraw -= DrawPhase;
    }

    //private void Start() => StartGame();


    void StartGame()
    {
        print($"Starting Game...");
        //Select 1st player
        ActivePlayerID = UnityEngine.Random.Range(0,players.Length);
        players[ActivePlayerID].isActive = true;
        NextTurnPhase();
    }

    public void NextTurnPhase()
    {
        cancelSource.Cancel();
        cancelSource = new();
        currentPhase++;

        if (FirstTurn)
        {
            currentPhase = TurnPhase.Play;
            FirstTurn = false;
        }

        var action = currentPhase switch
        {
            TurnPhase.Upkeep => OnUpkeep,
            TurnPhase.Play => OnPlay,
            TurnPhase.Combat => OnCombat,
            TurnPhase.Discard => OnDiscard,
            TurnPhase.Draw => OnDraw,
            _ => null
        };
        if (action == null) print($"Invalid Turn Phase {currentPhase}");
        action?.Invoke(ActivePlayerID);
        
    }

    public async void UpkeepPhase(int ActivePlayerID)
    {
        print($"Player {ActivePlayerID}'s {currentPhase} Phase...");
        players[ActivePlayerID].Resources = 0;
        if (players[ActivePlayerID].lanes.Length != 0)
        {
            foreach (var lane in players[ActivePlayerID].lanes)
            {
                if (lane != null)
                    await lane.card.OnUpkeep();
            }
        }
        else
        {
            await Task.Delay(5000);
        }
        NextTurnPhase();
    }
    private async void PlayPhase(int ActivePlayerID)
    {
        print($"Player {ActivePlayerID}'s {currentPhase} Phase...");
        await Timer(PlayTimer, null, cancelSource.Token);
        
        NextTurnPhase();
    }

    public async void CombatPhase(int ActivePlayerID)
    {
        print($"Player {ActivePlayerID}'s {currentPhase} Phase...");
        if (players[ActivePlayerID].lanes.Length != 0)
        {
            print($"{players[ActivePlayerID].lanes.Length} Lanes found");
            foreach (var lane in players[ActivePlayerID].lanes)
            {
                if (lane != null)
                    await lane.card.OnCombat();
            }
        }
        else
        {
            await Timer(PlayTimer, null, cancelSource.Token);
        }
        NextTurnPhase();
    }

    private async void DiscardPhase(int ActivePlayerID)
    {
        print($"Player {ActivePlayerID}'s {currentPhase} Phase...");
        await Timer(PlayTimer, null, cancelSource.Token);

        NextTurnPhase();
    }

    private async void DrawPhase(int ActivePlayerID)
    {
        print($"Player {ActivePlayerID}'s {currentPhase} Phase...");
        await Timer(PlayTimer, null, cancelSource.Token);
        EndTurn();
    }

    void EndTurn()
    {
        print($"End of Player {ActivePlayerID}'s Turn...");
        players[ActivePlayerID].isActive = false;
        OnEndTurn?.Invoke(ActivePlayerID);
        ActivePlayerID++;
        ActivePlayerID %= players.Length;
        if(FirstTurn) FirstTurn = false;
        players[ActivePlayerID].isActive = true;
        currentPhase = TurnPhase.EndTurn;        
        NextTurnPhase();
    }

    public async Task Timer(float amount, IProgress<float> progress, CancellationToken cancelToken)
    {
        for (int i = 0; i <= amount; i++)
        {
            if (cancelToken.IsCancellationRequested) break;
            await Task.Delay(1000);
            progress?.Report(i / amount);
        }
        print("Times Up!");
    }

    private void OnApplicationQuit() => cancelSource.Cancel();
}
