using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameActionLogger : MonoBehaviour
{
    public static GameActionLogger Instance
    {
        get; private set;
    }

    List<ActionLog> ActionLogs;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is already a GameActionLogger: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ActionLogs = new List<ActionLog>();
    }

    public void LogAction(string actionName)
    {
        ActionLog actionLog = new ActionLog
        {
            actionName = actionName,
            playerTurn = TurnSystem.Instance.IsPlayerTurn(),
            turnNumber = TurnSystem.Instance.GetTurnNumber()
        };

        ActionLogs.Add(actionLog);
    }

    public void SaveLogToFile()
    {
        string path = GetPath();
        WriteFile(path);
    }

    private void WriteFile(string path)
    {
        Debug.Log("Saving game log file...");
        StreamWriter writer = new StreamWriter(path);

        writer.WriteLine("Action,PlayerTurn,Turn");

        foreach (ActionLog actionLog in ActionLogs)
        {
            string actionLogString = actionLog.actionName + "," + actionLog.playerTurn.ToString() + "," + actionLog.turnNumber.ToString();
            writer.WriteLine(actionLogString);
        }

        writer.Flush();
        writer.Close();
    }

    private string GetPath()
    {
        string path = "GameLogs/";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        int numberOfFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;

        path = path + "GameLog" + numberOfFiles.ToString() + ".csv";

        return path;
    }
}
