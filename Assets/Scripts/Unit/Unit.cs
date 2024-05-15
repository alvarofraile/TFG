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
    private bool gameFinished = false;

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
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void GameManager_OnGameEnd(object sender, GameManager.OnGameEndEventArgs e)
    {
        /*
        UnitAgent agent = GetComponent<UnitAgent>();

        if(agent != null)
        {
            AssignGameResultReward(e.gameResult, agent);
        }
        */
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
        UnitAgent agent = GetComponent<UnitAgent>();
        agent.AddReward(-500);

        string cumulativeReward = "Cumulative Reward = " + GetComponent<UnitAgent>().GetCumulativeReward().ToString();
        print(cumulativeReward);

        LevelGrid.Instance.RemoveUnitFromTilePosition(tilePosition, this);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
        Destroy(gameObject, 0.5f);
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

    public Unit GetClosestEnemy()
    {
        List<Unit> enemyUnits = UnitController.Instance.GetEnemyUnits();

        if (enemyUnits.Count == 0)
        {
            return null;
        }
        
        Unit closestEnemy = enemyUnits[0];

        float closestEnemyDistance = Vector3.Distance(GetWorldPosition(), closestEnemy.GetWorldPosition());

        foreach (Unit enemy in enemyUnits)
        {
            float distance = Vector3.Distance(GetWorldPosition(), enemy.GetWorldPosition());


            if (distance < closestEnemyDistance)
            {
                closestEnemy = enemy;
                closestEnemyDistance = distance;
            }
        }

        return closestEnemy;
    }

    public void TakeAgentAction(UnitAgent.UnitAgentActions action)
    {
        Debug.Log(action);

        UnitAgent agent = GetComponent<UnitAgent>();

        switch (action)
        {
            case UnitAgent.UnitAgentActions.Shoot:
            {
            ShootAction shootAction = this.GetAction<ShootAction>();

                Unit target = this.GetClosestEnemy();

                //Check that enemy is within shooting range
                float distanceToTarget = Vector3.Distance(target.transform.position, GetWorldPosition());
                if (distanceToTarget > shootAction.GetMaximumRange())
                {
                    return;
                }

                if (!this.TryUseActionPointsToTakeAction(shootAction))
                {
                    return;
                }

                //Reward
                int damageAmount = Mathf.Min(shootAction.GetDamageAmount(), target.GetRemainingHealth());
                bool isLethalHit = target.GetHealth().IsLethalHit(damageAmount);
                int eliminationBonus = 200;

                int reward = damageAmount + ((isLethalHit ? 1 : 0) * eliminationBonus);

                agent.AddReward(reward);

                //Take Action
                shootAction.TakeAction(target.tilePosition, UnitActionSystem.Instance.ClearBusy);
                UnitActionSystem.Instance.InvokeOnActionStarted();
                break;
                }
            case UnitAgent.UnitAgentActions.MoveOffense:
                {
                    MoveAction moveAction = this.GetAction<MoveAction>();
                    if (!this.TryUseActionPointsToTakeAction(moveAction))
                    {
                        return;
                    }

                    TilePosition targetTilePosition = tilePosition + new TilePosition(0, 2);

                    moveAction.TakeAction(moveAction.GetBestOffesinveTile(), UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();
                    break;
                }
            case UnitAgent.UnitAgentActions.MoveDefense:
                {
                    MoveAction moveAction = this.GetAction<MoveAction>();
                    if (!this.TryUseActionPointsToTakeAction(moveAction))
                    {
                        return;
                    }

                    TilePosition targetTilePosition = tilePosition + new TilePosition(0, -2);

                    moveAction.TakeAction(moveAction.GetBestDefensiveTile(), UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();
                    break;
                }
            case UnitAgent.UnitAgentActions.Melee:
                {
                    MeleeAction meleeAction = this.GetAction<MeleeAction>();

                    Unit target = this.GetClosestEnemy();

                    //Check that enemy is within melee range
                    float distanceToTarget = Vector3.Distance(target.transform.position, GetWorldPosition());
                    if (distanceToTarget > meleeAction.GetMaxMeleeDistance())
                    {
                        return;
                    }

                    if (!this.TryUseActionPointsToTakeAction(meleeAction))
                    {
                        return;
                    }

                    //Reward
                    int damageAmount = Mathf.Min(meleeAction.GetDamageAmount(), target.GetRemainingHealth());
                    bool isLethalHit = target.GetHealth().IsLethalHit(damageAmount);
                    int eliminationBonus = 200;

                    int reward = damageAmount + ((isLethalHit ? 1 : 0) * eliminationBonus);

                    agent.AddReward(reward);

                    meleeAction.TakeAction(target.tilePosition, UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();
                    break;
                }
            case UnitAgent.UnitAgentActions.Heal:
                {
                    HealAction healAction = this.GetAction<HealAction>();
                    if (!this.TryUseActionPointsToTakeAction(healAction))
                    {
                        return;
                    }

                    healAction.TakeAction(tilePosition, UnitActionSystem.Instance.ClearBusy);
                    UnitActionSystem.Instance.InvokeOnActionStarted();
                    break;
                }
        }
    }

    private void AssignGameResultReward(GameManager.GameResults gameResult, UnitAgent agent)
    {
        switch (gameResult)
        {
            case GameManager.GameResults.Draw:
                {
                    agent.AddReward(-1500);
                    break;
                }
            case GameManager.GameResults.Loss:
                {
                    agent.AddReward(-5000);
                    break;
                }
            case GameManager.GameResults.Win:
                {
                    agent.AddReward(5000);
                    break;
                }

        }
    }
}
