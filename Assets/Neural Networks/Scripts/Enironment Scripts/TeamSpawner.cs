using System.Collections.Generic;
using UnityEngine;

public class TeamSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject agentPrefab;

    [Header("Faction")]
    public FactionType faction;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public int maxAgents = 3;

    [Header("Patrol")]
    public Area patrolArea;

    private List<GameObject> aliveAgents =
        new List<GameObject>();

    private void Start()
    {
        SpawnMissingAgents();
    }

    private void Update()
    {
        CleanupDeadAgents();

        if (aliveAgents.Count < maxAgents)
        {
            SpawnMissingAgents();
        }
    }

    // =========================================================
    // CLEANUP
    // Removes destroyed agents from list
    // =========================================================
    private void CleanupDeadAgents()
    {
        aliveAgents.RemoveAll(agent => agent == null);
    }

    // =========================================================
    // SPAWNING
    // =========================================================
    private void SpawnMissingAgents()
    {
        int needed = maxAgents - aliveAgents.Count;

        for (int i = 0; i < needed; i++)
        {
            SpawnAgent();
        }
    }

    private void SpawnAgent()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                $"[SPAWNER] No spawn points assigned for {faction}"
            );
            return;
        }

        // Pick random spawn point
        Transform spawn =
            spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

        // Spawn agent
        GameObject newAgent =
            Instantiate(
                agentPrefab,
                spawn.position,
                spawn.rotation
            );

        
        //To load elite brains into new agents spawned to fill after after agent death
        // =====================================================
        // LOAD EVOLVED BRAIN INTO NEW SPAWN
        // =====================================================

        if (faction == FactionType.Alpha)
        {
            BrainSaveData data =
                EvolutionSaveManager.Instance.LoadBrains();

            if (data != null &&
                data.eliteBrains.Count > 0)
            {
                CopyNeuralNetwork nn =
                    newAgent.GetComponent<CopyNeuralNetwork>();

                if (nn != null)
                {
                    // Pick random elite
                    EliteBrainData brain =
                        data.eliteBrains[
                            Random.Range(
                                0,
                                data.eliteBrains.Count
                            )
                        ];

                    // LOAD BRAIN
                    nn.LoadBrain(
                        brain.weights,
                        brain.biases
                    );

                    // RESTORE MMR
                    AgentRewardSystem reward =
                        newAgent.GetComponent<AgentRewardSystem>();

                    if (reward != null)
                    {
                        reward.MMR = brain.mmr;
                    }

                    // MUTATE CHILD
                    NeuralMutation mutation =
                        newAgent.GetComponent<NeuralMutation>();

                    if (mutation != null)
                    {
                        mutation.MutateCreature();
                    }

                    Debug.Log(
                        $"[EVOLUTION] Spawned evolved child: " +
                        newAgent.name
                    );
                }
            }
        }

        // =====================================================
        // ASSIGN FACTION
        // =====================================================
        AgentIdentity identity =
            newAgent.GetComponent<AgentIdentity>();

        if (identity != null)
        {
            identity.Faction = faction;
        }

        // =====================================================
        // ASSIGN PATROL AREA
        // =====================================================
        PatrolRoute patrol =
            newAgent.GetComponent<PatrolRoute>();

        if (patrol != null)
        {
            patrol.patrolArea = patrolArea;
        }

        // =====================================================
        // RENAME AGENT
        // =====================================================
        newAgent.name =
            $"{faction}_Agent_{aliveAgents.Count + 1}";

        // =====================================================
        // TRACK AGENT
        // =====================================================
        aliveAgents.Add(newAgent);

        Debug.Log(
            $"[SPAWNER] Spawned {newAgent.name}"
        );
    }
}