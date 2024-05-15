using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    /*
     * Modelo determinista de IA enemiga:
     * 
     * Este modelo se basa en una maquina de estados con 3 posibles estados
     *      i. WaitingForTurn -> En este estado no es el turno del enemigo y debemos esperar
     *      ii. TakingTurn -> En este estado se decidirá, si es posible, que accion se realizará.
     *                        En caso de no poder realizar ninguna acción se avanzará al siguiente turno.
     *      iii. Busy -> Se entrará en este estado mientras las acciones tomadas estén siendo realizadas.
     *      
     * Para determinar que acciones se realizarán, calcularemos, para cada acción posible una puntuación cuyo calculo se
     * mostrará y explicará en los métodos relevantes. Estas puntuaciones representarán lo buena decisión que 
     * sería realizar esa acción. A partir de estas puntuaciones se decidirá que acción realizar.
     */

    public static EnemyAI Instance
    {
        get; private set;
    }

    private enum State
    {
        WaitingForTurn,
        TakingTurn,
        Busy
    }

    private State state;

    private float timer;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is already an EnemyAI: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        state = State.WaitingForTurn;
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        switch (state)
        {
            case State.WaitingForTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    state = State.Busy;
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        //No more enemies have available actions
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }

    }

    public void ResetState()
    {
        state = State.WaitingForTurn;
    }

    private void SetStateTakingTurn()
    {
        timer = .5f;
        state = State.TakingTurn;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            timer = 2f;
            state = State.TakingTurn;
        }

    }

    private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
    {
        foreach (Unit enemyUnit in UnitController.Instance.GetEnemyUnits())
        {
            if (TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        foreach (BaseAction baseAction in enemyUnit.GetBaseActions())
        {
            if (!enemyUnit.CanAffordAction(baseAction))
            {
                //Enemy cannot afford this action
                continue;
            }

            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (testEnemyAIAction != null && testEnemyAIAction.actionScore > bestEnemyAIAction.actionScore)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }
        }

        if (bestEnemyAIAction != null && enemyUnit.TryUseActionPointsToTakeAction(bestBaseAction))
        {
            bestBaseAction.TakeAction(bestEnemyAIAction.tilePosition, onEnemyAIActionComplete);
            return true;
        }
        else
        {
            return false;
        }
    }
}
