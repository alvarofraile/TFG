using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private int maxMoveDistance = 4;
    [SerializeField] private bool visualization = false;

    private List<Vector3> worldPositions;
    private int currentPositionIndex;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        Vector3 targetPos = worldPositions[currentPositionIndex];
        Vector3 moveDirection = (targetPos - transform.position).normalized;

        float stoppingDistance = 0.5f;

        if(Vector3.Distance(targetPos, transform.position) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            currentPositionIndex++;
            if(currentPositionIndex >= worldPositions.Count){
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                isActive = false;
                onActionFinished();
            }
        }

        float rotationSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        List<TilePosition> pathTilePositions = Pathfinding.Instance.FindPath(unit.GetTilePosition(), tilePosition, out int pathLenght);
        if(Pathfinding.Instance.GetShowDebugVisuals()){
            //TODO -> Mostrar Ruta con colores
            GridSystemVisualization.Instance.HideAllTileVisuals();
            GridSystemVisualization.Instance.ShowList(pathTilePositions, GridSystemVisualization.TileVisualType.Green);
        }
        currentPositionIndex = 0;
        worldPositions = new List<Vector3>();

        foreach(TilePosition pathTilePosition in pathTilePositions){
            worldPositions.Add(LevelGrid.Instance.GetWorldPosition(pathTilePosition));
        }

        if(visualization){
            GridSystemVisualization.Instance.ShowList(pathTilePositions, GridSystemVisualization.TileVisualType.Green);
            Time.timeScale = 0;
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    public override List<TilePosition> GetValidTilePositions()
    {
        List<TilePosition> validTilePositionList = new List<TilePosition>();

        TilePosition unitTilePosition = unit.GetTilePosition();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                TilePosition offsetTilePosition = new TilePosition(x, z);
                TilePosition testTilePosition = unitTilePosition + offsetTilePosition;

                if (!LevelGrid.Instance.IsValidTilePosition(testTilePosition))
                {
                    continue;
                }

                if (unitTilePosition == testTilePosition)
                {
                    //Same Tile Position where the unit already is
                    continue;
                }

                if (LevelGrid.Instance.HasUnitsOnTilePosition(testTilePosition))
                {
                    //Tile Position already occupied by another Unit
                    continue;
                }
                
                if (!Pathfinding.Instance.IsWalkableGridPosition(testTilePosition))
                {
                    continue;
                }
                
                if (!Pathfinding.Instance.HasPath(unitTilePosition, testTilePosition))
                {
                    continue;
                }

                int pathFindingDistanceMultiplier = 10;
                if (Pathfinding.Instance.GetPathLenght(unitTilePosition, testTilePosition) > maxMoveDistance * pathFindingDistanceMultiplier)
                {
                    //Path lenght is too long
                    continue;
                }


                validTilePositionList.Add(testTilePosition);
            }
        }

        return validTilePositionList;
    }

    public override EnemyAIAction GetEnemyAIAction(TilePosition tilePosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtTilePosition(tilePosition);

        return new EnemyAIAction
        {
            tilePosition = tilePosition,
            actionScore = targetCountAtGridPosition * 10,
        };
    }

    //TODO
    public TilePosition GetBestDefensiveTile(out int rating)
    {
        List<TilePosition> validTilePositions = GetValidTilePositions();

        int bestDefensiveRating = int.MinValue;
        float bestDistanceToClosestEnemy = float.MinValue;
        TilePosition bestTilePosition = validTilePositions[0];

        foreach(TilePosition tilePosition in validTilePositions)
        {
            int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtTilePosition(tilePosition);
            int allyCountAtGridPosition = unit.GetNearbyAlliesCounInRange(4);

            int defensiveRating = allyCountAtGridPosition - targetCountAtGridPosition;

            float distanceToClosestEnemyAtTilePosition = Vector3.Distance(unit.GetWorldPosition(),
                                                        unit.GetClosestEnemyAtTilePosition(tilePosition).GetWorldPosition());

            if (defensiveRating > bestDefensiveRating ||
               (defensiveRating > bestDefensiveRating && bestDistanceToClosestEnemy < distanceToClosestEnemyAtTilePosition))
            {
                bestTilePosition = tilePosition;
                bestDefensiveRating = defensiveRating;
                bestDistanceToClosestEnemy = distanceToClosestEnemyAtTilePosition;
            }
        }

        rating = bestDefensiveRating;
        return bestTilePosition;
    }

    public TilePosition GetBestOffesinveTile(out int rating)
    {
        List<TilePosition> validTilePositions = GetValidTilePositions();

        int bestTargetCount = int.MinValue;
        int bestAllyCount = int.MinValue;
        float bestDistanceToClosestEnemy = float.MaxValue;
        TilePosition bestTilePosition = validTilePositions[0];

        foreach (TilePosition tilePosition in validTilePositions)
        {
            int targetCountAtTilePosition = unit.GetAction<ShootAction>().GetTargetCountAtTilePosition(tilePosition);
            int allyCountAtTilePosition = unit.GetNearbyAlliesCounInRange(4);
            float distanceToClosestEnemyAtTilePosition = Vector3.Distance(unit.GetWorldPosition(), 
                                                         unit.GetClosestEnemyAtTilePosition(tilePosition).GetWorldPosition());

            if (targetCountAtTilePosition > bestTargetCount || 
                (targetCountAtTilePosition == bestTargetCount && bestAllyCount < allyCountAtTilePosition) ||
                (targetCountAtTilePosition == bestTargetCount && bestAllyCount == allyCountAtTilePosition && bestDistanceToClosestEnemy > distanceToClosestEnemyAtTilePosition))
            {
                bestTilePosition = tilePosition;
                bestTargetCount = targetCountAtTilePosition;
                bestAllyCount = allyCountAtTilePosition;
                bestDistanceToClosestEnemy = distanceToClosestEnemyAtTilePosition;
            }
        }

        rating = (bestTargetCount - bestAllyCount);

        return bestTilePosition;
    }
}
