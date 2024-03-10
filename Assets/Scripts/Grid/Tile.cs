using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{

    private GridSystem gridSystem;
    private TilePosition tilePosition;
    private List<Unit> units;

    public Tile(GridSystem gridSystem, TilePosition tilePosition)
    {
        this.gridSystem = gridSystem;
        this.tilePosition = tilePosition;
        units= new List<Unit>();
    }

    public override string ToString()
    {
        string unitsString = "Units on tile: \n";

        /*
        foreach (Unit unit in units) 
        { 
            unitsString += unit.ToString() + "\n";
        }
        */

        return tilePosition.ToString() + "\n" + unitsString;
    }

    public List<Unit> GetUnits()
    {
        return units;
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
    }

    public bool hasAnyUnit()
    {
        return units.Count > 0;
    }

    public Unit GetUnit()
    {
        if (hasAnyUnit())
        {
            return units[0];
        }
        else
        {
            return null;
        }
    }
}
