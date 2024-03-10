using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public static UnitController Instance
    {
        get; private set;
    }

    private List<Unit> allUnits;
    private List<Unit> friendlyUnits;
    private List<Unit> enemyUnits;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is already an UnitController: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        allUnits = new List<Unit>();
        friendlyUnits = new List<Unit>();
        enemyUnits = new List<Unit>();
    }

    private void Start()
    {
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.T)) {
            Debug.Log(allUnits.Count);
        }
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        allUnits.Add(unit);

        if (unit.IsEnemy())
        {
            enemyUnits.Add(unit);
        }
        else
        {
            friendlyUnits.Add(unit);
        }
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        allUnits.Remove(unit);

        if (unit.IsEnemy())
        {
            enemyUnits.Remove(unit);
        }
        else
        {
            friendlyUnits.Remove(unit);
        }
    }

    public List<Unit> GetUnits() { return allUnits; }

    public List<Unit> GetEnemyUnits() { return enemyUnits; }

    public List<Unit> GetFriendlyUnits() { return friendlyUnits; }
}
