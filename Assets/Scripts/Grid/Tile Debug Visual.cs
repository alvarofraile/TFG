using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileDebugVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro positionText;
    private object tile;

    public virtual void SetTile(object tile) 
    {
        this.tile = tile;
    }

    protected virtual void Update()
    {
        positionText.text = tile.ToString();
    }
}
