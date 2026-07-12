using UnityEngine;

/// <summary>
/// Reproduces NPC when reward threshold is reached.
/// Clone spawns with FULL HEALTH.
/// </summary>

public class AgentReproduction : MonoBehaviour
{
    [Header("References")]
    public AgentRewardSystem rewardSystem;

    [Header("Reproduction")]
    public float reproductionThreshold = 300f;

    [Header("Spawn Settings")]
    public float spawnOffset = 2f;

    private bool isReproducing = false;

    private void Awake()
    {
        if (rewardSystem == null)
        {
            rewardSystem =
                GetComponent<AgentRewardSystem>();
        }
    }

    private void Update()
    {
        if (rewardSystem == null)
            return;

        if (isReproducing)
            return;

        if (rewardSystem.totalReward >= reproductionThreshold)
        {
            Reproduce();
        }
    }

    private static int cloneCounter = 1;

    private void Reproduce()
    {
        isReproducing = true;

        // ====================================
        // RANDOM SPAWN POSITION
        // ====================================

        Vector3 spawnPos =
            transform.position +
            Random.insideUnitSphere * spawnOffset;

        spawnPos.y = transform.position.y;

        // ====================================
        // CREATE CLONE
        // ====================================

        GameObject clone =
            Instantiate(
                gameObject,
                spawnPos,
                transform.rotation
            );
        NeuralMutation mutation =
        clone.GetComponent<NeuralMutation>();

        if (mutation != null)
        {
            mutation.MutateCreature();
        }

        clone.name =
            gameObject.name + "_" + cloneCounter;
            cloneCounter++;

        // ====================================
        // RESET PARENT REWARDS
        // ====================================

        rewardSystem.ResetRewards();

        // ====================================
        // RESET CLONE REWARDS
        // ====================================

        AgentRewardSystem cloneRewards =
            clone.GetComponent<AgentRewardSystem>();

        if (cloneRewards != null)
        {
            cloneRewards.ResetRewards();
        }

        // ====================================
        // RESET CLONE HEALTH TO FULL
        // ====================================

        HealthComponent cloneHealth =
            clone.GetComponent<HealthComponent>();

        if (cloneHealth != null)
        {
            cloneHealth.ResetHealthComponent();
        }

        Debug.Log(
            $"{gameObject.name} reproduced successfully!"
        );

        isReproducing = false;
    }
}