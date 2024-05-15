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
        int gameBalance = GameManager.Instance.GetGameBalance();
        int targetsInShootingRange = unit.GetAction<ShootAction>().GetTargetCountAtTilePosition(unit.GetTilePosition());
        int nearbyAllies = unit.GetNearbyAlliesCounInRange(4);
        float health = unit.GetHealthNormalized();

        float closestEnemyDistance;
        float closestEnemyHealth;
        Unit closestEnemy = unit.GetClosestEnemy();
        if(closestEnemy != null)
        {
            closestEnemyDistance = Vector3.Distance(this.unit.GetWorldPosition(), closestEnemy.GetWorldPosition());
            closestEnemyHealth = closestEnemy.GetHealthNormalized();
        }
        else
        {
            closestEnemyDistance = 0;
            closestEnemyHealth = 0;
        }


        sensor.AddObservation(gameBalance);
        sensor.AddObservation(targetsInShootingRange);
        sensor.AddObservation(nearbyAllies);
        sensor.AddObservation(health);
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
                float distance = Vector3.Distance(this.unit.GetWorldPosition(), this.unit.GetClosestEnemy().GetWorldPosition());
                if (distance < unit.GetAction<ShootAction>().GetMaxShootingDistance())
                {
                    unit.TakeAgentAction(UnitAgentActions.Shoot);
                }
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
        print("Episode");
        unit = gameObject.GetComponent<Unit>();
    }
}
