using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static ShootAction;

public class HealAction : BaseAction
{
    public event EventHandler OnHeal;

    [SerializeField] private int healAmount = 50;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        Heal();

        ActionFinished();
    }

    private void Heal()
    {
        unit.Heal(healAmount);
        OnHeal?.Invoke(this, EventArgs.Empty);
    }

    public override string GetActionName()
    {
        return "Heal";
    }

    public override List<TilePosition> GetValidTilePositions()
    {
        List<TilePosition> validTilePositions = new List<TilePosition>();

        TilePosition tilePosition = unit.GetTilePosition();
        validTilePositions.Add(tilePosition);

        return validTilePositions;
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);
    }

}
