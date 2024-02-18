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

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionFinished()
    {
        isActive = false;
        onActionFinished();

        OnAnyActionFinished?.Invoke(this, EventArgs.Empty);
    }
}
