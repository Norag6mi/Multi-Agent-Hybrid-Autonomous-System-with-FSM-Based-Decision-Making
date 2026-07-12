using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EvolutionMetricsManager : MonoBehaviour
{
    public static EvolutionMetricsManager Instance;

    [Header("Logging")]
    public float logInterval = 10f;

    private float timer;

    private string csvPath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        csvPath =
            Application.persistentDataPath +
            "/EvolutionMetrics.csv";
    }

    private void Start()
    {
        CreateFile();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            timer = 0f;
            LogSnapshot();
        }
    }

    private void CreateFile()
    {
        if (File.Exists(csvPath))
            File.Delete(csvPath);

        string header =
            "Time," +
            "AlphaAlive," +
            "BetaAlive," +
            "AverageMMR," +
            "BestMMR," +
            "AlphaKills," +
            "BetaKills," +
            "TotalReproductions\n";

        File.WriteAllText(csvPath, header);

        Debug.Log("Metrics CSV Created");
    }

    private void LogSnapshot()
    {
        AgentRewardSystem[] agents =
            FindObjectsOfType<AgentRewardSystem>();

        int alphaAlive = 0;
        int betaAlive = 0;

        List<float> alphaMMRs =
            new List<float>();

        foreach (AgentRewardSystem reward in agents)
        {
            HealthComponent health =
                reward.GetComponent<HealthComponent>();

            if (health == null)
                continue;

            if (health.Model.IsDead)
                continue;

            AgentIdentity identity =
                reward.GetComponent<AgentIdentity>();

            if (identity == null)
                continue;

            if (identity.Faction == FactionType.Alpha)
            {
                alphaAlive++;
                alphaMMRs.Add(reward.MMR);
            }
            else if (identity.Faction == FactionType.Beta)
            {
                betaAlive++;
            }
        }

        float avgMMR = 0f;
        float bestMMR = 0f;

        if (alphaMMRs.Count > 0)
        {
            avgMMR = alphaMMRs.Average();
            bestMMR = alphaMMRs.Max();
        }

        // ==============================
        // Kill Metrics
        // ==============================

        int alphaKills = 0;
        int betaKills = 0;

        if (ResearchKillTracker.Instance != null)
        {
            alphaKills =
                ResearchKillTracker.Instance.GetAlphaKills();

            betaKills =
                ResearchKillTracker.Instance.GetBetaKills();
        }

        // ==============================
        // Reproduction Metrics
        // ==============================

        int reproductions = 0;

        ResearchReproductionTracker reproductionTracker =
            FindObjectOfType<ResearchReproductionTracker>();

        if (reproductionTracker != null)
        {
            reproductions =
                reproductionTracker.GetTotalReproductions();
        }

        // ==============================
        // Write Row
        // ==============================

        string row =
            Time.time.ToString("F1") + "," +
            alphaAlive + "," +
            betaAlive + "," +
            avgMMR.ToString("F2") + "," +
            bestMMR.ToString("F2") + "," +
            alphaKills + "," +
            betaKills + "," +
            reproductions +
            "\n";

        File.AppendAllText(csvPath, row);

        Debug.Log(
            $"[METRICS] " +
            $"Alpha:{alphaAlive} " +
            $"Beta:{betaAlive} " +
            $"AvgMMR:{avgMMR:F1} " +
            $"BestMMR:{bestMMR:F1} " +
            $"Kills A:{alphaKills} " +
            $"Kills B:{betaKills} " +
            $"Repro:{reproductions}"
        );
    }

    public string GetCSVPath()
    {
        return csvPath;
    }
}