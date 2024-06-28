using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class UnitAgentController : MonoBehaviour
{
    public static UnitAgentController Instance
    {
        private set; get;
    }

    private List<Vector3> enemyPositions = new List<Vector3>();
    [SerializeField] private Transform enemyPrefab;
    private List<Vector3> unitAgentPositions = new List<Vector3>();
    [SerializeField] private Transform unitAgentPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitAgentController. " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SaveUnitPositions();
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;
    }

    private void GameManager_OnGameEnd(object sender, GameManager.OnGameEndEventArgs e)
    {
        EndEpisode(e.gameResult);
    }

    public Unit GetAgent()
    {
        List<Unit> friendlyUnits = UnitController.Instance.GetFriendlyUnits();

        //Comprobar unidades con suficientes puntos para realizar una accion
        List<Unit> availableUnits = new List<Unit>();
        foreach(Unit unit in friendlyUnits)
        {
            BaseAction[] unitActions = unit.GetBaseActions();
            int lowestActionCost = int.MaxValue;
            foreach(BaseAction action in unitActions)
            {
                if(action.GetCost() < lowestActionCost)
                {
                    lowestActionCost = action.GetCost();
                }
            }

            if(unit.GetActionPoints() >= lowestActionCost)
            {
                availableUnits.Add(unit);
            }
        }

        if (availableUnits.Count == 0)
        {
            //No quedan unidadades vivas
            return null;
        }

        int randomUnitIndex = Random.Range(0, availableUnits.Count - 1);
        return availableUnits[randomUnitIndex];
    }

    private void EndEpisode(GameManager.GameResults gameResult)
    {
        List<Unit> friendlyUnits = UnitController.Instance.GetFriendlyUnits();

        foreach (Unit unit in friendlyUnits)
        {
            if(gameResult == GameManager.GameResults.Win){
                unit.gameObject.GetComponent<UnitAgent>().AddReward(10000f);
            }
            else if(gameResult == GameManager.GameResults.Loss){
                unit.gameObject.GetComponent<UnitAgent>().AddReward(-10000f);
            }
            else{
                //Draw
                unit.gameObject.GetComponent<UnitAgent>().AddReward(-2000f);
            }

            string cumulativeReward = "Cumulative Reward = " + unit.gameObject.GetComponent<UnitAgent>().GetCumulativeReward().ToString();
            print(cumulativeReward);
            //unit.gameObject.GetComponent<UnitAgent>().EndEpisode();
        }

        ResetTrainingScenario();
    }

    private void SaveUnitPositions()
    {
        List<Unit> enemyUnits = UnitController.Instance.GetEnemyUnits();

        foreach (Unit unit in enemyUnits)
        {
            enemyPositions.Add(unit.gameObject.transform.position);
        }

        List<Unit> friendlyUnits = UnitController.Instance.GetFriendlyUnits();

        foreach (Unit unit in friendlyUnits)
        {
            unitAgentPositions.Add(unit.gameObject.transform.position);
        }
    }

    private void ResetTrainingScenario()
    {
        //Remove all enemies and friendly units
        List<Unit> enemyUnits = UnitController.Instance.GetEnemyUnits();
        List<Unit> friendlyUnits = UnitController.Instance.GetFriendlyUnits();

        foreach (Unit unit in friendlyUnits)
        {
            LevelGrid.Instance.RemoveUnitFromTilePosition(unit.GetTilePosition(), unit);
            Destroy(unit.gameObject);
        }

        foreach (Unit unit in enemyUnits)
        {
            LevelGrid.Instance.RemoveUnitFromTilePosition(unit.GetTilePosition(), unit);
            Destroy(unit.gameObject);
        }

        UnitController.Instance.Clear();

        //Instantiate enemies and friendly units in their original locations

        foreach (Vector3 enemyPosition in enemyPositions)
        {
            Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);
        }

        foreach (Vector3 unitPosition in unitAgentPositions)
        {
            Transform a = Instantiate(unitAgentPrefab, unitPosition, Quaternion.identity);
        }

        //Reset turn
        TurnSystem.Instance.ResetTurn();

        //Clear action system busy state
        UnitActionSystem.Instance.ClearBusy();
    }
}
