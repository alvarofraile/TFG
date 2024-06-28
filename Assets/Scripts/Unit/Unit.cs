using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
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
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

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


    public TilePosition GetTilePosition()
    {
        return tilePosition;
    }

    public Vector3 GetWorldPosition()
    {
        return LevelGrid.Instance.GetWorldPosition(GetTilePosition());
        //return gameObject.transform.position;
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

        if(TryGetComponent<UnitAgent>(out UnitAgent agent))
        {
            agent.AddReward(-damageAmount);
        }
    }

    public void Heal(int healAmount)
    {
        unitHealth.Heal(healAmount);
    }

    public float GetHealthNormalized()
    {
        return unitHealth.GetHealthNormalized();
    }

    public int GetRemainingHealth()
    {
        return unitHealth.GetRemainingHealth();
    }

    private void UnitHealt_OnDead(object sender, EventArgs e)
    {
        if(TryGetComponent<UnitAgent>(out UnitAgent agent))
        {
            agent.AddReward(-100);

            string cumulativeReward = "Cumulative Reward = " + agent.GetCumulativeReward().ToString();
            print(cumulativeReward);
        }

        LevelGrid.Instance.RemoveUnitFromTilePosition(tilePosition, this);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
        Destroy(gameObject, 0.1f);
    }

    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in baseActions)
        {
            if (baseAction is T)
            {
                return baseAction as T;
            }
        }

        return null;
    }

//TODO
    public int GetNearbyAlliesCounInRange(int range)
    {
        int nearbyAllies = 0;

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                TilePosition offset = new TilePosition(x, z);
                TilePosition tilePosition = this.tilePosition + offset;

                if (!LevelGrid.Instance.IsValidTilePosition(tilePosition))
                {
                    continue;
                }

                float distance = new Vector2(Math.Abs(x), Math.Abs(z)).magnitude;
                if (distance > range)
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasUnitsOnTilePosition(tilePosition))
                {
                    //Casilla vacia
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);
                if (targetUnit.IsEnemy() != this.IsEnemy())
                {
                    //Ambas unidades en diferentes equipos
                    continue;
                }

                nearbyAllies++;

            }
        }

        return nearbyAllies;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) ||
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = maxActionPoints;

            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    //AGENT---------------------------------------------------------
//TODO
    public Unit GetClosestEnemyAtTilePosition(TilePosition tilePosition)
    {
        List<Unit> enemyUnits = UnitController.Instance.GetEnemyUnits();

        if (enemyUnits.Count == 0)
        {
            return null;
        }

        Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(tilePosition);
        Unit closestEnemy = enemyUnits[0];

        float closestEnemyDistance = Vector3.Distance(worldPos, closestEnemy.GetWorldPosition());

        foreach (Unit enemy in enemyUnits)
        {
            float distance = Vector3.Distance(worldPos, enemy.GetWorldPosition());


            if (distance < closestEnemyDistance)
            {
                closestEnemy = enemy;
                closestEnemyDistance = distance;
            }
        }

        return closestEnemy;
    }
//TODO
    public void TakeAgentAction(UnitAgent.UnitAgentActions action)
    {
        Debug.Log(action);

        UnitAgent agent = GetComponent<UnitAgent>();

        switch (action)
        {
            case UnitAgent.UnitAgentActions.Shoot:
                {
                    ShootAction shootAction = this.GetAction<ShootAction>();

                    Unit target = this.GetClosestEnemyAtTilePosition(GetTilePosition());

                    if(shootAction.IsValidTileForShootingAction(target.GetTilePosition(), this.GetTilePosition()) & this.TryUseActionPointsToTakeAction(shootAction)){
                        //Valid Tile Position for Shooting -> Execute Action & Reward
                        int damageAmount = Mathf.Min(shootAction.GetDamageAmount(), target.GetRemainingHealth());
                        bool isLethalHit = target.GetHealth().IsLethalHit(damageAmount);
                        int eliminationBonus = 300;

                        int reward = damageAmount * 3 + ((isLethalHit ? 1 : 0) * eliminationBonus);

                        agent.AddReward(reward);
                        
                        //Take Action
                        UnitActionSystem.Instance.SetBusy();
                        shootAction.TakeAction(target.tilePosition, UnitActionSystem.Instance.ClearBusy);
                        UnitActionSystem.Instance.InvokeOnActionStarted();
                    }
                    break;
                }
            case UnitAgent.UnitAgentActions.MoveOffense:
                {
                    MoveAction moveAction = this.GetAction<MoveAction>();
                    if (!this.TryUseActionPointsToTakeAction(moveAction))
                    {
                        return;
                    }

                    UnitActionSystem.Instance.SetBusy();
                    moveAction.TakeAction(moveAction.GetBestOffesinveTile(out int rating), UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();

                    float reward = rating * 10;
                    agent.AddReward(reward);

                    break;
                }
            case UnitAgent.UnitAgentActions.MoveDefense:
                {
                    MoveAction moveAction = this.GetAction<MoveAction>();
                    if (!this.TryUseActionPointsToTakeAction(moveAction))
                    {
                        return;
                    }

                    UnitActionSystem.Instance.SetBusy();
                    moveAction.TakeAction(moveAction.GetBestDefensiveTile(out int rating), UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();

                    float reward = rating * 10;
                    agent.AddReward(reward);

                    break;
                }
            case UnitAgent.UnitAgentActions.Melee:
                {
                    MeleeAction meleeAction = this.GetAction<MeleeAction>();

                    Unit target = this.GetClosestEnemyAtTilePosition(GetTilePosition());

                    //Check that enemy is within melee range
                    float distanceToTarget = Vector3.Distance(target.transform.position, GetWorldPosition());
                    if (distanceToTarget > meleeAction.GetMaxMeleeDistance() | !this.TryUseActionPointsToTakeAction(meleeAction))
                    {
                        return;
                    }

                    //Take Action
                    UnitActionSystem.Instance.SetBusy();
                    meleeAction.TakeAction(target.tilePosition, UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();

                    //Reward
                    int reward = 600;

                    agent.AddReward(reward);

                    break;
                }
            case UnitAgent.UnitAgentActions.Heal:
                {
                    HealAction healAction = this.GetAction<HealAction>();
                    if (!this.TryUseActionPointsToTakeAction(healAction))
                    {
                        return;
                    }

                    //Take Action
                    UnitActionSystem.Instance.SetBusy();
                    healAction.TakeAction(tilePosition, UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();

                    //Reward
                    float reward = (1f - unitHealth.GetHealthNormalized()) * 100;

                    agent.AddReward(reward);

                    break;
                }
        }
    }
}
