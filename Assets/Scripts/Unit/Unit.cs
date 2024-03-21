using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    /*
     * TODO: Event -> turnChanged
     */

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    [SerializeField] private int maxActionPoints;
    [SerializeField] private bool isEnemy;

    private TilePosition tilePosition;
    private UnitHealth unitHealth;
    private BaseAction[] baseActions;
    private int actionPoints;

    private void Awake()
    {
        unitHealth = GetComponent<UnitHealth>();
        baseActions = GetComponents<BaseAction>();

        tilePosition = LevelGrid.Instance.GetTilePosition(transform.position);
        LevelGrid.Instance.AddUnitToTilePosition(tilePosition, this);

        actionPoints = maxActionPoints;
    }

    private UnitHealth GetHealth()
    {
        return unitHealth;
    }

    void Start()
    {
        unitHealth.OnDead += UnitHealt_OnDead;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    void Update()
    {
        TilePosition newTilePosition = LevelGrid.Instance.GetTilePosition(transform.position);

        if(newTilePosition != tilePosition)
        {
            TilePosition oldTilePosition = tilePosition;
            tilePosition = newTilePosition;

            LevelGrid.Instance.MoveUnitTilePosition(this, oldTilePosition, newTilePosition);
        }
    }

    public T getAction<T>() where T : BaseAction
    {
        foreach (BaseAction action in baseActions)
        {
            if(action is T)
            {
                return action as T;
            }
        }

        return null;
    }

    public TilePosition GetTilePosition()
    {
        return tilePosition;
    }

    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    }

    public BaseAction[] GetBaseActions()
    {
        return baseActions;
    }

    public bool CanAffordAction(BaseAction action)
    {
        if(actionPoints >= action.GetCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UseActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool TryUseActionPointsToTakeAction(BaseAction action)
    {
        if(CanAffordAction(action))
        {
            UseActionPoints(action.GetCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public void Damage(int damageAmount)
    {
        unitHealth.Damage(damageAmount);
    }

    public void Heal(int healAmount)
    {
        unitHealth.Heal(healAmount);
    }

    public float GetHealthNormalized()
    {
        return unitHealth.GetHealthNormalized();
    }

    private void UnitHealt_OnDead(object sender, EventArgs e)
    {
        //TODO -> Muerte
        throw new NotImplementedException();
    }
}
