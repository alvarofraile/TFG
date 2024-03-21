using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction
{
    public override string GetActionName()
    {
        return "Melee";
    }

    public override List<TilePosition> GetValidTilePositions()
    {
        throw new NotImplementedException();
    }

    public override void TakeAction(TilePosition tilePosition, Action onActionComplete)
    {
        throw new NotImplementedException();
    }
}
