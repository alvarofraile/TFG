using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance{
        get;
        private set;
    }

    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private bool showDebugVisuals;
    [SerializeField] private LayerMask obstacleLayer;

    private int width;
    private int height;
    private float tileSize;
    private GridSystem<PathfindingTile> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one Pathfinding. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float tileSize){
        
        gridSystem = new GridSystem<PathfindingTile>(width, height, tileSize, 
            (GridSystem<PathfindingTile> g, TilePosition tilePosition) => new PathfindingTile(tilePosition));
        
        if(showDebugVisuals){
            gridSystem.ShowDebugVisuals(gridDebugObjectPrefab);
        }

        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < height; z++)
            {
                TilePosition tilePosition = new TilePosition(x, z);
                Vector3 worldPostion = LevelGrid.Instance.GetWorldPosition(tilePosition);
                float raycastOffsetDistance = 5f;
                if(Physics.Raycast(worldPostion + Vector3.down * raycastOffsetDistance, Vector3.up, raycastOffsetDistance * 2, obstacleLayer))
                {
                    GetPathfindingTile(x, z).SetIsWalkable(false);
                }
            }
        }
    }

    public List<TilePosition> FindPath(TilePosition startTilePosition, TilePosition endTilePosition, out int pathLenght){
        List<PathfindingTile> openList = new List<PathfindingTile>();
        List<PathfindingTile> closedList = new List<PathfindingTile>();

        PathfindingTile startTile = gridSystem.GetTile(startTilePosition);
        PathfindingTile endTile = gridSystem.GetTile(endTilePosition);
        openList.Add(startTile);

        for(int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for(int z = 0; z < gridSystem.GetHeight(); z++)
            {
                TilePosition tilePosition = new TilePosition(x, z);
                PathfindingTile pathfindingTile = gridSystem.GetTile(tilePosition);
                
                pathfindingTile.SetGCost(int.MaxValue);
                pathfindingTile.SetHCost(0);
                pathfindingTile.CalculateFCost();
                pathfindingTile.ResetCameFromPathfindigTile();
            }
        }

        startTile.SetGCost(0);
        startTile.SetHCost(CalculateDistance(startTilePosition, endTilePosition));
        startTile.CalculateFCost();

        while(openList.Count > 0){
            PathfindingTile currentTile = GetLowestFCostPathfindingTile(openList);

            if(currentTile == endTile){
                pathLenght = endTile.GetFCost();
                return CalculatePath(endTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach(PathfindingTile neighbourTile in GetNeighbourTiles(currentTile))
            {
                if(closedList.Contains(neighbourTile)){
                    continue;
                }

                if(!neighbourTile.IsWalkable()){
                    closedList.Add(neighbourTile);
                    continue;
                }

                int testGCost = currentTile.GetGCost() + CalculateDistance(currentTile.GetTilePosition(), neighbourTile.GetTilePosition());

                if(testGCost < neighbourTile.GetGCost()){
                    neighbourTile.SetCameFromPathfindingTile(currentTile);
                    neighbourTile.SetGCost(testGCost);
                    neighbourTile.SetHCost(CalculateDistance(neighbourTile.GetTilePosition(), endTilePosition));
                    neighbourTile.CalculateFCost();

                    if(!openList.Contains(neighbourTile)){
                        openList.Add(neighbourTile);
                    }
                }
            }
        }

        //No se ha encontrado ningun camino posible
        pathLenght = 0;
        return null;
    }

    private PathfindingTile GetPathfindingTile(int x, int z)
    {
        return gridSystem.GetTile(new TilePosition(x, z));
    }

    public int CalculateDistance(TilePosition tilePositionA, TilePosition tilePositionB)
    {
        TilePosition tilePositionDistance = tilePositionA - tilePositionB;

        int xDistance = Mathf.Abs(tilePositionDistance.x);
        int zDistance = Mathf.Abs(tilePositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);

        return DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + STRAIGHT_COST * remaining;
    }

    private PathfindingTile GetLowestFCostPathfindingTile(List<PathfindingTile> pathfindingTiles)
    {
        PathfindingTile lowestFCostPathfindingTile = pathfindingTiles[0];

        for(int i = 0; i < pathfindingTiles.Count; i++)
        {
            if (pathfindingTiles[i].GetFCost() < lowestFCostPathfindingTile.GetFCost())
            {
                lowestFCostPathfindingTile = pathfindingTiles[i];
            }
        }

        return lowestFCostPathfindingTile;
    }

    private List<PathfindingTile> GetNeighbourTiles(PathfindingTile currentTile)
    {
        List<PathfindingTile> neighbours = new List<PathfindingTile>();

        TilePosition tilePosition = currentTile.GetTilePosition();

        if(tilePosition.x - 1 >= 0)
        {
            //Lado Izquierdo
            neighbours.Add(GetPathfindingTile(tilePosition.x - 1, tilePosition.z + 0));

            if (tilePosition.z - 1 >= 0)
            {
                //Izq. Abajo
                neighbours.Add(GetPathfindingTile(tilePosition.x - 1, tilePosition.z - 1));
            }

            if (tilePosition.z + 1 < gridSystem.GetHeight())
            {
                //Izq. Arriba
                neighbours.Add(GetPathfindingTile(tilePosition.x - 1, tilePosition.z + 1));
            }
        }

        if(tilePosition.x + 1 < gridSystem.GetWidth())
        {
            //Lado Derecho
            neighbours.Add(GetPathfindingTile(tilePosition.x + 1, tilePosition.z + 0));

            if (tilePosition.z - 1 >= 0)
            {
                //Der. Abajo
                neighbours.Add(GetPathfindingTile(tilePosition.x + 1, tilePosition.z - 1));
            }

            if (tilePosition.z + 1< gridSystem.GetHeight())
            {
                //Der. Arriba
                neighbours.Add(GetPathfindingTile(tilePosition.x + 1, tilePosition.z + 1));
            }
        }

        if (tilePosition.z - 1 >= 0)
        {
            //Abajo
            neighbours.Add(GetPathfindingTile(tilePosition.x + 0, tilePosition.z - 1));
        }

        if (tilePosition.z + 1< gridSystem.GetHeight())
        {
            //Arriba
            neighbours.Add(GetPathfindingTile(tilePosition.x + 0, tilePosition.z + 1));
        }

        return neighbours;
    }

    private List<TilePosition> CalculatePath(PathfindingTile endTile)
    {
        List<PathfindingTile> pathfindingTiles = new List<PathfindingTile>();

        pathfindingTiles.Add(endTile);

        PathfindingTile currentTile = endTile;

        while(currentTile.GetCameFromPathfindigTile() != null) 
        { 
            pathfindingTiles.Add(currentTile.GetCameFromPathfindigTile());
            currentTile = currentTile.GetCameFromPathfindigTile();
        }

        pathfindingTiles.Reverse();

        List<TilePosition> tilePositions = new List<TilePosition>();
        foreach(PathfindingTile pathfindingTile in pathfindingTiles)
        {
            tilePositions.Add(pathfindingTile.GetTilePosition());
        }

        return tilePositions;
    }

    public bool IsWalkableGridPosition(TilePosition tilePosition)
    {
        return gridSystem.GetTile(tilePosition).IsWalkable();
    }

    public bool HasPath(TilePosition startTilePosition, TilePosition endTilePosition)
    {
        return FindPath(startTilePosition, endTilePosition, out int pathLenght) != null;
    }

    public int GetPathLenght(TilePosition startTilePosition, TilePosition endTilePosition) 
    {
        FindPath(startTilePosition, endTilePosition, out int pathLenght);
        return pathLenght;
    }

    public bool GetShowDebugVisuals(){
        return showDebugVisuals;
    }
}
