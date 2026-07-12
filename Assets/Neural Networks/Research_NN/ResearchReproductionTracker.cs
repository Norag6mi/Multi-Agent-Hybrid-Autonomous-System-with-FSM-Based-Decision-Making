using UnityEngine;
using System.IO;

public class ResearchReproductionTracker : MonoBehaviour
{
    private string csvPath;

    private int previousAlphaCount;

    private int reproductionEvents;

    private float timer;

    [Header("Logging")]
    public float checkInterval = 1f;

    public int GetTotalReproductions()
    {
        return reproductionEvents;
    }

    private void Start()
    {
        csvPath =
            Application.persistentDataPath +
            "/ReproductionMetrics.csv";

        if (!File.Exists(csvPath))
        {
            File.WriteAllText(
                csvPath,
                "Time,AlphaPopulation,ReproductionEvents\n"
            );
        }

        previousAlphaCount = CountAlphaAgents();

        Debug.Log(
            $"Initial Alpha Count: {previousAlphaCount}"
        );
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < checkInterval)
            return;

        timer = 0f;

        int currentAlphaCount =
            CountAlphaAgents();

        if (currentAlphaCount > previousAlphaCount)
        {
            int newAgents =
                currentAlphaCount -
                previousAlphaCount;

            reproductionEvents += newAgents;

            string row =
                Time.time.ToString("F1") + "," +
                currentAlphaCount + "," +
                reproductionEvents +
                "\n";

            File.AppendAllText(csvPath, row);

            Debug.Log(
                $"Reproduction Detected! +" +
                $"{newAgents} Alpha Agents"
            );
        }

        previousAlphaCount =
            currentAlphaCount;
    }

    private int CountAlphaAgents()
    {
        // FIXED: Added () syntax
        AgentIdentity[] agents =
            FindObjectsOfType<AgentIdentity>();

        int count = 0;

        foreach (AgentIdentity agent in agents)
        {
            HealthComponent health =
                agent.GetComponent<HealthComponent>();

            if (health == null)
                continue;

            if (health.Model.IsDead)
                continue;

            // FIXED: Capitalized Faction to match AgentIdentity
            string faction =
                agent.Faction.ToString();

            if (faction.Contains("Alpha"))
            {
                count++;
            }
        }

        return count;
    }
}
