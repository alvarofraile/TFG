using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTile
{
    private TilePosition tilePosition;

    private int gCost;
    private int hCost;
    private int fCost;
    private PathfindingTile cameFromPathfindingTile;
    private bool isWalkable = true;

    public PathfindingTile(TilePosition tilePosition)
    {
        this.tilePosition = tilePosition;
    }

    public override string ToString()
    {
        return tilePosition.ToString();
    }

    public int GetGCost()
    {
        return gCost;
    }

    public int GetHCost()
    {
        return hCost;
    }

    public int GetFCost()
    {
        return fCost;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void ResetCameFromPathfindigTile()
    {
        cameFromPathfindingTile = null;
    }

    public void SetCameFromPathfindingTile(PathfindingTile pathfindingTile)
    {
        cameFromPathfindingTile = pathfindingTile;
    }
    
    public PathfindingTile GetCameFromPathfindigTile() 
    { 
        return cameFromPathfindingTile;
    }

    public TilePosition GetTilePosition()
    {
        return tilePosition;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
}
