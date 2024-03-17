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

    private enum State
    {
        Aiming,
        Shooting,
        Ending
    }

    private State state;
    [SerializeField] private int maxShootDistance = 5;
    [SerializeField] private int damageAmount = 30;
    private float stateTimer;
    private Unit target;
    private bool canShoot;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch(state)
        {
            case State.Aiming:
                Vector3 aimDirection = (target.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotationSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotationSpeed);
                break;
            case State.Shooting:
                if (canShoot)
                {
                    Shoot();
                    canShoot = false;
                }
                break;
            case State.Ending:
                ActionFinished();
                break;
        }

        if(stateTimer <= 0f)
        {
            NextState();
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

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingTime = 0.1f;
                stateTimer = shootingTime;
                break;
            case State.Shooting:
                state = State.Ending;
                float endTime = 0.5f;
                stateTimer = endTime;
                break;
            case State.Ending:
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
        List<TilePosition> validTilePositions = new List<TilePosition>();

        TilePosition unitTilePosition = unit.GetTilePosition();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                TilePosition offset = new TilePosition(x, z);
                TilePosition tilePosition = unitTilePosition + offset;

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
                    //Casilla vacía
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);
                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    //Ambas unidades en el mismo equipo
                    continue;
                }

                //TODO -> Comprobar Obstaculos

                validTilePositions.Add(tilePosition);

            }
        }

        return validTilePositions;
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        target = LevelGrid.Instance.GetUnitAtTilePosition(tilePosition);

        state = State.Aiming;
        float aimingTime = 0.5f;
        stateTimer = aimingTime;

        canShoot = true;

        ActionStart(onActionComplete);
    }
}
