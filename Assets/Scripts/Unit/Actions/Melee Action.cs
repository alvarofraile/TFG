using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction
{
    public event EventHandler OnMeleeActionStarted;
    public static event EventHandler OnAnyMeleeHit;

    private const int MAX_MELEE_ACTION_RANGE = 1;

    private enum Phase
    {
        BeforeHit,
        AfterHit,
    }

    [SerializeField] private int damage = 90;
    private Phase phase;
    private float phaseTimer;

    private Unit target;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        phaseTimer -= Time.deltaTime;

        switch(phase)
        {
            case Phase.BeforeHit:
                Vector3 aimDir = (target.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotateSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
                break;
            case Phase.AfterHit:
                break;
        }

        if(phaseTimer < 0)
        {
            NextPhase();
        }
    }

    private void NextPhase()
    {
        switch(phase)
        {
            case Phase.BeforeHit:
                phase = Phase.AfterHit;
                float afterHitPhaseTime = 0.5f;
                phaseTimer = afterHitPhaseTime;
                target.Damage(damage);
                OnAnyMeleeHit?.Invoke(this, EventArgs.Empty);
                break;
            case Phase.AfterHit:
                ActionFinished();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    public override List<TilePosition> GetValidTilePositions()
    {
        List<TilePosition> validTilePositionList = new List<TilePosition>();

        TilePosition unitTilePosition = unit.GetTilePosition();

        for (int x = -MAX_MELEE_ACTION_RANGE; x <= MAX_MELEE_ACTION_RANGE; x++)
        {
            for (int z = -MAX_MELEE_ACTION_RANGE; z <= MAX_MELEE_ACTION_RANGE; z++)
            {
                TilePosition offsetTilePosition = new TilePosition(x, z);
                TilePosition tilePosition = unitTilePosition + offsetTilePosition;

                if (!LevelGrid.Instance.IsValidTilePosition(tilePosition))
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasUnitsOnTilePosition(tilePosition))
                {
                    //Tile Position is empty, no Unit
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    //Both Units on the same "team"
                    continue;
                }

                validTilePositionList.Add(tilePosition);
            }
        }

        return validTilePositionList;
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        target = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);

        phase = Phase.BeforeHit;
        float beforeHitPhaseTime = 0.7f;
        phaseTimer = beforeHitPhaseTime;

        OnMeleeActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    public int GetMaxMeleeDistance()
    {
        return MAX_MELEE_ACTION_RANGE;
    }

    public override EnemyAIAction GetEnemyAIAction(TilePosition tilePosition)
    {
        return new EnemyAIAction
        {
            tilePosition = tilePosition,
            actionScore = 200,
        };
    }
}
