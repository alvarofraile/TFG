using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathfindigDebugVisualization : TileDebugVisual
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private SpriteRenderer isWalkableIndicator;

    private PathfindingTile pathfindingTile;

    public override void SetTile(object tile)
    {
        base.SetTile(tile);
        pathfindingTile = (PathfindingTile)tile;
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = pathfindingTile.GetGCost().ToString();
        hCostText.text = pathfindingTile.GetHCost().ToString();
        fCostText.text = pathfindingTile.GetFCost().ToString();
        isWalkableIndicator.color = pathfindingTile.IsWalkable() ? Color.green : Color.red;
    }
}
