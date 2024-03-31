using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

public class GridSystem<TTile>
{

    private int width;
    private int height;
    private float tileSize;
    private TTile[,] tileArray;


    public GridSystem(int width, int height, float tileSize, Func<GridSystem<TTile>, TilePosition, TTile> createTile) 
    { 
        this.width = width;
        this.height = height;
        this.tileSize = tileSize;

        tileArray = new TTile[width, height];

        InitializeTileArray(createTile);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    private void InitializeTileArray(Func<GridSystem<TTile>, TilePosition, TTile> createTile)
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TilePosition tilePosition = new TilePosition(i, j);
                tileArray[i, j] = createTile(this, tilePosition);
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

    public TTile GetTile(TilePosition tilePosition)
    {
        return tileArray[tilePosition.x, tilePosition.z];
    }

    public TTile GetTile(Vector3 worldPosition)
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

    public void ShowDebugVisuals(Transform tileDebugVisualPrefab) 
    { 
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TilePosition tilePosition = new TilePosition(i, j);

                Transform tileDebugVisual = GameObject.Instantiate(tileDebugVisualPrefab, GetWorldPosition(tilePosition), Quaternion.identity, LevelGrid.Instance.transform);
                TileDebugVisual gridDebugObject = tileDebugVisual.GetComponent<TileDebugVisual>();
                gridDebugObject.SetTile(GetTile(tilePosition));
            }
        }
    }

}
