using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    public event EventHandler<OnShootEventArgs> OnShoot;
    public static event EventHandler<OnShootEventArgs> OnAnyShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shooterUnit;
    }

    private enum Phase
    {
        Aiming,
        Shooting,
        Ending
    }

    private Phase phase;
    [SerializeField] private int maxShootDistance = 5;
    [SerializeField] private int damageAmount = 30;
    [SerializeField] private LayerMask obstacleLayer;
    private float phaseTimer;
    private Unit target;
    private bool canShoot;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        phaseTimer -= Time.deltaTime;

        switch(phase)
        {
            case Phase.Aiming:
                Vector3 aimDirection = (target.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotationSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotationSpeed);
                break;
            case Phase.Shooting:
                if (canShoot)
                {
                    Shoot();
                    canShoot = false;
                }
                break;
            case Phase.Ending:
                ActionFinished();
                break;
        }

        if(phaseTimer <= 0f)
        {
            NextPhase();
        }
    }

    private void Shoot()
    {
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = target,
            shooterUnit = unit
        });

        OnAnyShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = target,
            shooterUnit = unit
        });

        target.Damage(damageAmount);
    }

    private void NextPhase()
    {
        switch (phase)
        {
            case Phase.Aiming:
                phase = Phase.Shooting;
                float shootingTime = 0.1f;
                phaseTimer = shootingTime;
                break;
            case Phase.Shooting:
                phase = Phase.Ending;
                float endTime = 0.5f;
                phaseTimer = endTime;
                break;
            case Phase.Ending:
                isActive = false;
                onActionFinished();
                break;

        }
    }

    public int GetMaxShootingDistance()
    {
        return maxShootDistance;
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<TilePosition> GetValidTilePositions()
    {
        TilePosition unitTilePosition = unit.GetTilePosition();
        return GetValidTilePositions(unitTilePosition);
    }

    public List<TilePosition> GetValidTilePositions(TilePosition originTilePosition)
    {
        List<TilePosition> validTilePositions = new List<TilePosition>();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                TilePosition offset = new TilePosition(x, z);
                TilePosition tilePosition = originTilePosition + offset;

                if(IsValidTileForShootingAction(tilePosition, originTilePosition)){
                    validTilePositions.Add(tilePosition);
                }
                else
                {
                    continue;
                }
            }
        }

        return validTilePositions;
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        target = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);

        phase = Phase.Aiming;
        float aimingTime = 0.5f;
        phaseTimer = aimingTime;

        canShoot = true;

        ActionStart(onActionComplete);
    }

    public override EnemyAIAction GetEnemyAIAction(TilePosition tilePosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);

        return new EnemyAIAction
        {
            tilePosition = tilePosition,
            actionScore = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }

    public int GetTargetCountAtTilePosition(TilePosition tilePosition)
    {
        return GetValidTilePositions(tilePosition).Count;
    }

    public int GetMaximumRange()
    {
        return maxShootDistance;
    }
    public int GetDamageAmount()
    {
        return damageAmount;
    }

    public bool IsValidTileForShootingAction(TilePosition tilePosition, TilePosition originTilePosition){
        if (!LevelGrid.Instance.IsValidTilePosition(tilePosition))
        {
            return false;
        }

        TilePosition offset = originTilePosition - tilePosition;
        float distance = new Vector2(Math.Abs(offset.x), Math.Abs(offset.z)).magnitude;
        if (distance > maxShootDistance)
        {
            return false;
        }

        if (!LevelGrid.Instance.HasUnitsOnTilePosition(tilePosition))
        {
            return false;
        }

        Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);
        if (targetUnit.IsEnemy() == unit.IsEnemy())
        {
            //Ambas unidades en el mismo equipo
            return false;
        }

        Vector3 shootingDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
        float unitShootingHeight = 1.7f;
        if(Physics.Raycast(unit.GetWorldPosition() + Vector3.up * unitShootingHeight, shootingDirection, Vector3.Distance(unit.GetWorldPosition(), targetUnit.GetWorldPosition()), obstacleLayer)){
            //Comprobar si hay obstaculos al disparar
            return false;
        }

        return true;
    }
}
