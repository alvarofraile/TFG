using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get; private set;
    }

    public class OnGameEndEventArgs : EventArgs
    {
        public GameResults gameResult;
    }

    public event EventHandler<OnGameEndEventArgs> OnGameEnd;

    [SerializeField] private int turnLimit = 50;
    [SerializeField] private bool logGame = false;

    public enum GameResults
    {
        Win,
        Loss,
        Draw
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is already a GameManager: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        CheckWin();
    }

    private void CheckWin()
    {
        int friendlyUnits = UnitController.Instance.GetFriendlyUnits().Count;
        int enemyUnits = UnitController.Instance.GetEnemyUnits().Count;

        if(friendlyUnits == 0)
        {
            EndGame(GameResults.Loss);
        }
        else if(enemyUnits == 0)
        {
            EndGame(GameResults.Win);
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        CheckTurnLimit();
    }

    private void CheckTurnLimit()
    {
        int turn = TurnSystem.Instance.GetTurnNumber();

        if(turn >= turnLimit)
        {
            EndGame(GameResults.Draw);
        }
    }

    private void EndGame(GameResults gameResult)
    {
        Debug.Log(gameResult);

        if(logGame){
            GameActionLogger.Instance.SaveLogToFile();
        }

        EnemyAI.Instance.ResetState();
        
        OnGameEnd?.Invoke(this, new OnGameEndEventArgs
        {
            gameResult = gameResult
        });
    }

    public int GetGameBalance()
    {
        int friendlyUnits = UnitController.Instance.GetFriendlyUnits().Count;
        int enemyUnits = UnitController.Instance.GetEnemyUnits().Count;


        if(friendlyUnits > enemyUnits)
        {
            return 1;
        }
        else if(friendlyUnits < enemyUnits)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}