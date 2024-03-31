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

    private PathfindingTile pathfindingNode;

    public override void SetTile(object tile)
    {
        base.SetTile(tile);
        pathfindingNode = (PathfindingTile)tile;
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = pathfindingNode.GetGCost().ToString();
        hCostText.text = pathfindingNode.GetHCost().ToString();
        fCostText.text = pathfindingNode.GetFCost().ToString();
        isWalkableIndicator.color = pathfindingNode.IsWalkable() ? Color.green : Color.red;
    }
}
