using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public GridSystem gridSystem;

    private void Start()
    { 
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        { 
            GridSystemVisualization.Instance.ShowRange(new TilePosition(2, 2), 2, GridSystemVisualization.TileVisualType.Green);   
        }
    }
}
