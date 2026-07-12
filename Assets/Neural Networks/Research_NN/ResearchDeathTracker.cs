using UnityEngine;
using System.IO;

public class ResearchDeathTracker : MonoBehaviour
{
    private HealthComponent health;
    private AgentFSM fsm;
    private AgentIdentity identity;

    private bool logged = false;

    private string csvPath;

    private string lastAliveState = "Unknown";

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        // FIXED: Added <AgentFSM>() syntax
        fsm = GetComponent<AgentFSM>();
        identity = GetComponent<AgentIdentity>();

        csvPath =
            Application.persistentDataPath +
            "/DeathStateMetrics.csv";
    }

    private void Start()
    {
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(
                csvPath,
                "Time,Faction,State\n"
            );
        }

        if (health != null)
        {
            health.OnDeathEvent += LogDeath;
        }
    }

    private void Update()
    {
        if (fsm == null)
            return;

        string currentState =
            fsm.CurrentStateKey.ToString();

        if (currentState != "Dead")
        {
            lastAliveState = currentState;
        }
    }

    private void LogDeath()
    {
        if (logged)
            return;

        logged = true;

        string faction = "Unknown";

        if (identity != null)
            // FIXED: Capitalized Faction to match AgentIdentity
            faction = identity.Faction.ToString();

        string row =
            Time.time.ToString("F1") + "," +
            faction + "," +
            lastAliveState + "\n";

        File.AppendAllText(csvPath, row);

        if (ResearchKillTracker.Instance != null)    // for research kill tracker to log kills based on deaths
        {
            ResearchKillTracker.Instance.RegisterDeath(identity.Faction);
        }

        Debug.Log(
            $"DEATH LOGGED | {faction} | {lastAliveState}"
        );
    }
}
