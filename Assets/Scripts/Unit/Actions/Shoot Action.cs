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

                if (!LevelGrid.Instance.IsValidTilePosition(tilePosition))
                {
                    continue;
                }

                float distance = new Vector2(Math.Abs(x), Math.Abs(z)).magnitude;
                if (distance > maxShootDistance)
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasUnitsOnTilePosition(tilePosition))
                {
                    //Casilla vac�a
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);
                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    //Ambas unidades en el mismo equipo
                    continue;
                }

                Vector3 shootingDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float unitShootingHeight = 1.7f;
                if(Physics.Raycast(unit.GetWorldPosition() + Vector3.up * unitShootingHeight, shootingDirection, Vector3.Distance(unit.GetWorldPosition(), targetUnit.GetWorldPosition()), obstacleLayer)){
                    //Comprobar si hay obstaculos al disparar
                    continue;
                }

                validTilePositions.Add(tilePosition);

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
}
