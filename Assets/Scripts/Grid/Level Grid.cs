using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{

    public static LevelGrid Instance { get; private set; }

    public event EventHandler OnAnyUnitMovedGridPosition;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float tileSize;
    [SerializeField] private bool debugVisualization;

    private GridSystem gridSystem;
    [SerializeField] private Transform tileDebugVisualPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LevelGrid. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystem = new GridSystem(width, height, tileSize);

        if(debugVisualization)
        {
            gridSystem.showDebugVisuals(tileDebugVisualPrefab);
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public GridSystem GetGridSystem()
    {
        //TODO: DELETE THIS METHOD
        return gridSystem;
    }

    public void AddUnitToTilePosition(TilePosition tilePosition, Unit unit)
    {
        Tile tile = gridSystem.GetTile(tilePosition);
        tile.AddUnit(unit);
    }

    public List<Unit> GetUnitsListAtTilePosition(TilePosition tilePosition)
    {
        Tile tile = gridSystem.GetTile(tilePosition);
        return tile.GetUnits();
    }

    public Unit GetUnitAtTilePosition(TilePosition tilePosition)
    {
        Tile tile = gridSystem.GetTile(tilePosition);
        return tile.GetUnit();
    }

    public void RemoveUnitFromTilePosition(TilePosition tilePosition, Unit unit)
    {
        Tile tile = gridSystem.GetTile(tilePosition);
        tile.RemoveUnit(unit);
    }

    public void MoveUnitTilePosition(Unit unit, TilePosition fromTilePosition, TilePosition toTilePosition)
    {
        RemoveUnitFromTilePosition(fromTilePosition, unit);

        AddUnitToTilePosition(toTilePosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    public bool HasUnitsOnTilePosition(TilePosition tilePosition)
    {
        Tile tile = gridSystem.GetTile(tilePosition);
        return tile.hasAnyUnit();
    }

    public TilePosition GetTilePosition(Vector3 worldPosition) => gridSystem.GetTilePosition(worldPosition);

    public Vector3 GetWorldPosition(TilePosition tilePosition) => gridSystem.GetWorldPosition(tilePosition);

    public bool IsValidTilePosition(TilePosition tilePosition) => gridSystem.IsValidTilePosition(tilePosition);
}
