using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionFinished;

    protected Unit unit;
    protected bool isActive;
    protected Action onActionFinished;

    [SerializeField] private int actionCost;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();

    public abstract void TakeAction(TilePosition tilePosition, Action onActionComplete);

    public virtual bool IsValidTilePosition(TilePosition tilePosition)
    {
        List<TilePosition> validTilePositions = GetValidTilePositions();
        return validTilePositions.Contains(tilePosition);
    }

    public abstract List<TilePosition> GetValidTilePositions();

    public virtual int GetCost()
    {
        return actionCost;
    }

    public Unit GetUnit()
    {
        return unit;
    }

    protected void ActionStart(Action OnActionComplete)
    {
        isActive = true;
        this.onActionFinished = OnActionComplete;

        GameActionLogger.Instance.LogAction(GetActionName());

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionFinished()
    {
        isActive = false;
        onActionFinished();

        OnAnyActionFinished?.Invoke(this, EventArgs.Empty);
    }

    //TODO
    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActions = new List<EnemyAIAction>();

        List<TilePosition> validActionTilePositions = GetValidTilePositions();

        foreach (TilePosition tilePositions in validActionTilePositions)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(tilePositions);
            enemyAIActions.Add(enemyAIAction);
        }

        if (enemyAIActions.Count > 0)
        {
            enemyAIActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionScore - a.actionScore);

            return enemyAIActions[0];
        }
        else
        {
            //No possible enemy AI actions
            return null;
        }

    }

    //TODO
    public abstract EnemyAIAction GetEnemyAIAction(TilePosition tilePosition);
}
