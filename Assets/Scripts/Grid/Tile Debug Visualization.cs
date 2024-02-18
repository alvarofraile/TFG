using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileDebugVisualization : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMeshPro;
    private Tile tile;

    public void SetTile(Tile tile) 
    {
        this.tile = tile;
    }

    private void Update()
    {
        textMeshPro.text = tile.ToString();
    }
}
