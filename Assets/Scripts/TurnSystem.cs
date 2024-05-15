using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance
    {
        get;
        private set;
    }

    public event EventHandler OnTurnChanged;

    private int turnCounter = 1;
    private bool isPlayerTurn = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void NextTurn()
    {
        turnCounter++;
        isPlayerTurn = !isPlayerTurn;

        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return turnCounter/2 + 1;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void ResetTurn()
    {
        turnCounter = 1;
        isPlayerTurn = true;
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }
}
