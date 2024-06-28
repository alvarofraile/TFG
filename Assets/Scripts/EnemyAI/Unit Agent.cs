using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class UnitAgent : Agent
{
    public enum UnitAgentActions
    {
        Shoot,
        MoveOffense,
        MoveDefense,
        Melee,
        Heal
    }

    [SerializeField] private Unit unit;

    private void Awake()
    {
    }

    private void Update()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int friendlyUnits = UnitController.Instance.GetFriendlyUnits().Count;
        int enemyUnits = UnitController.Instance.GetEnemyUnits().Count;
        int turnNumber = TurnSystem.Instance.GetTurnNumber();
        int targetsInShootingRange = unit.GetAction<ShootAction>().GetTargetCountAtTilePosition(unit.GetTilePosition());
        int nearbyAllies = unit.GetNearbyAlliesCounInRange(4);
        float health = unit.GetHealthNormalized();
        int availableActionPoints = unit.GetActionPoints();

        float closestEnemyDistance;
        float closestEnemyHealth;
        Unit closestEnemy = unit.GetClosestEnemyAtTilePosition(unit.GetTilePosition());
        if(closestEnemy != null)
        {
            closestEnemyDistance = Vector3.Distance(this.unit.GetWorldPosition(), closestEnemy.GetWorldPosition());
            closestEnemyHealth = closestEnemy.GetHealthNormalized();
        }
        else
        {
            //En estos casos la partida debería haber acabado
            closestEnemyDistance = 0;
            closestEnemyHealth = 0;
        }


        sensor.AddObservation(friendlyUnits);
        sensor.AddObservation(enemyUnits);
        sensor.AddObservation(turnNumber);
        sensor.AddObservation(targetsInShootingRange);
        sensor.AddObservation(nearbyAllies);
        sensor.AddObservation(health);
        sensor.AddObservation(availableActionPoints);
        sensor.AddObservation(closestEnemyDistance);
        sensor.AddObservation(closestEnemyHealth);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Read continuous actions
        List<float> continuousActions = actions.ContinuousActions.ToList();
        string list = "ML Output:";
        foreach (float item in continuousActions)
        {
            list += item.ToString();
            list += ", ";
        }
        print(list);

        //Get maximum value position
        int bestAction = continuousActions.IndexOf(continuousActions.Max());
        //Transform position into action to perform
        switch (bestAction)
        {
            case 0:
                //Dispara al enemigo mas cercano si esta en rango
                unit.TakeAgentAction(UnitAgentActions.Shoot);
                break;
            case 1:
                unit.TakeAgentAction(UnitAgentActions.MoveOffense);
                break;
            case 2:
                unit.TakeAgentAction(UnitAgentActions.MoveDefense);
                break;
            case 3:
                unit.TakeAgentAction(UnitAgentActions.Melee);
                break;
            case 4:
                unit.TakeAgentAction(UnitAgentActions.Heal);
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        print("Episode Begin");
        unit = gameObject.GetComponent<Unit>();
    }
}
