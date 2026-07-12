using UnityEngine;
using System.IO;

public class ResearchKillTracker : MonoBehaviour
{
    public static ResearchKillTracker Instance;

    private string csvPath;

    private int alphaKills;
    private int betaKills;

    private bool fileCreated = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        csvPath =
            Application.persistentDataPath +
            "/KillMetrics.csv";

        if (!File.Exists(csvPath))
        {
            File.WriteAllText(
                csvPath,
                "Time,AlphaKills,BetaKills\n"
            );

            fileCreated = true;
        }
    }

    public void RegisterDeath(FactionType deadFaction)
    {
        if (deadFaction == FactionType.Beta)
        {
            alphaKills++;
        }
        else if (deadFaction == FactionType.Alpha)
        {
            betaKills++;
        }

        SaveSnapshot();
    }

    private void SaveSnapshot()
    {
        string row =
            Time.time.ToString("F1") + "," +
            alphaKills + "," +
            betaKills + "\n";

        File.AppendAllText(csvPath, row);
    }

    public int GetAlphaKills()
    {
        return alphaKills;
    }

    public int GetBetaKills()
    {
        return betaKills;
    }
}