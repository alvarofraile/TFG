using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GridSystem
{

    private int width;
    private int height;
    private float tileSize;
    private Tile[,] tileArray;


    public GridSystem(int width, int height, float tileSize) 
    { 
        this.width = width;
        this.height = height;
        this.tileSize = tileSize;

        tileArray = new Tile[width, height];

        InitializeTileArray();
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    private void InitializeTileArray()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TilePosition tilePosition = new TilePosition(i, j);
                tileArray[i, j] = new Tile(this, tilePosition);
            }
        }
    }

    public Vector3 GetWorldPosition(TilePosition tilePosition)
    {
        return new Vector3(tilePosition.x, 0, tilePosition.z) * tileSize;
    }

    public TilePosition GetTilePosition(Vector3 worldPosition)
    {
        return new TilePosition(
            Mathf.RoundToInt(worldPosition.x / tileSize),
            Mathf.RoundToInt(worldPosition.z / tileSize)
            );
    }

    public Tile GetTile(TilePosition tilePosition)
    {
        return tileArray[tilePosition.x, tilePosition.z];
    }

    public Tile GetTile(Vector3 worldPosition)
    {
        TilePosition tilePosition = GetTilePosition(worldPosition);
        return tileArray[tilePosition.x, tilePosition.z];
    }

    public bool IsValidTilePosition(TilePosition tilePosition)
    {
        return  tilePosition.x >= 0 &&
                tilePosition.z >= 0 &&
                tilePosition.x < width &&
                tilePosition.z < height;
    }

    public void showDebugVisuals(Transform tileDebugVisualPrefab) 
    { 
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TilePosition tilePosition = new TilePosition(i, j);

                Transform tileDebugVisual = GameObject.Instantiate(tileDebugVisualPrefab, GetWorldPosition(tilePosition), Quaternion.identity, LevelGrid.Instance.transform);
                TileDebugVisualization gridDebugObject = tileDebugVisual.GetComponent<TileDebugVisualization>();
                gridDebugObject.SetTile(GetTile(tilePosition));
            }
        }
    }

}
