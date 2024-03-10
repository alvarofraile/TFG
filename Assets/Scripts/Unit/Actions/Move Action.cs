using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private int maxMoveDistance = 4;

    private Vector3 targetPos;

    protected override void Awake()
    {
        base.Awake();
        targetPos = transform.position;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        Vector3 moveDirection = (targetPos - transform.position).normalized;

        float stoppingDistance = 0.1f;

        if(Vector3.Distance(targetPos, transform.position) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            //TODO: Animacion andar
        }
        else
        {
            isActive = false;
            onActionFinished();
            //TODO: Parar animacion andar
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
        this.onActionFinished = onActionComplete;
        this.targetPos = LevelGrid.Instance.GetWorldPosition(tilePosition);
        isActive = true;
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

                validTilePositionList.Add(testTilePosition);
            }
        }

        return validTilePositionList;
    }
}
