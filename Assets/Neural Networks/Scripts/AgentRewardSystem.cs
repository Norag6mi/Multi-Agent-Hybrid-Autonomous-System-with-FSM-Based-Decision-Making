using UnityEngine;

public class AgentRewardSystem : MonoBehaviour
{
    [Header("Reward Values")]
    public float hitReward = 1f;

    public float damageRewardMultiplier = 0.1f;

    public float killReward = 50f;

    // =====================================================
    // TEMPORARY REWARD
    // Used for current survival cycle
    // Resets every survival check interval
    // =====================================================

    [Header("Current Reward")]
    public float totalReward = 0f;

    // =====================================================
    // PERSISTENT MMR
    // Long-term accumulated score
    // Used later for:
    // - evolution
    // - ranking
    // - selection
    // =====================================================

    [Header("MMR for Rank Calculation")]
    public float MMR = 0f;

    // =====================================================
    // STATS
    // =====================================================

    [Header("Stats")]
    public int totalHits;

    public int totalKills;

    public int totalDamageDealt;

    // =====================================================
    // SURVIVAL PRESSURE SYSTEM
    // IMPORTANT:
    // This can be ENABLED / DISABLED anytime
    // from Inspector.
    //
    // Purpose:
    // Prevent passive coward behavior.
    //
    // Agents MUST earn enough reward
    // within a time limit to survive.
    // =====================================================

    [Header("Survival Pressure")]

    [Tooltip("Enable/Disable forced survival combat system")]
    public bool useSurvivalPressure = true;

    [Tooltip("How often reward check happens")]
    public float survivalCheckInterval = 30f;

    [Tooltip("Minimum reward needed to survive")]
    public float requiredReward = 100f;

    // Internal timer
    private float survivalTimer;

    // =====================================================
    // REWARD FUNCTIONS
    // =====================================================

    public void RewardHit(int damage)
    {
        totalHits++;

        totalDamageDealt += damage;

        float reward =
            hitReward +
            (damage * damageRewardMultiplier);

        // Temporary reward
        totalReward += reward;

        // Persistent reward
        MMR += reward;

        Debug.Log(
            $"{gameObject.name} gained HIT reward. " +
            $"Reward: {totalReward} | MMR: {MMR}"
        );
    }

    public void RewardKill()
    {
        totalKills++;

        totalReward += killReward;

        MMR += killReward;

        Debug.Log(
            $"{gameObject.name} gained KILL reward. " +
            $"Reward: {totalReward} | MMR: {MMR}"
        );
    }

    [Header("Penalties")]
    public float obstacleCollisionPenalty = 2f;

    public void PenalizeObstacleCollision()
    {
        totalReward -= obstacleCollisionPenalty;
        MMR -= obstacleCollisionPenalty;

        Debug.Log(
            $"{gameObject.name} hit obstacle. " +
            $"Penalty: {obstacleCollisionPenalty} | " +
            $"Reward: {totalReward} | " +
            $"MMR: {MMR}"
        );
    }

    // =====================================================
    // SURVIVAL PRESSURE UPDATE
    // =====================================================

    private void Update()
    {
        // ============================================
        // SYSTEM DISABLED
        // ============================================

        if (!useSurvivalPressure)
            return;

        // ============================================
        // TIMER
        // ============================================

        survivalTimer += Time.deltaTime;

        // ============================================
        // CHECK EVERY X SECONDS
        // ============================================

        if (survivalTimer >= survivalCheckInterval)
        {
            // Reset timer
            survivalTimer = 0f;

            Debug.Log(
                $"{gameObject.name} Survival Check | " +
                $"Reward: {totalReward} / {requiredReward}"
            );

            // ========================================
            // FAILED SURVIVAL CHECK
            // ========================================

            if (totalReward < requiredReward)
            {
                Debug.Log(
                    $"{gameObject.name} FAILED survival pressure and died."
                );

                ForceDeath();
            }

            // ========================================
            // RESET TEMPORARY REWARD
            // For next cycle only
            // ========================================

            totalReward = 0f;
        }
    }

    // =====================================================
    // FORCE DEATH
    // ONLY affects agents using this script
    // =====================================================

    private void ForceDeath()
    {
        HealthComponent health =
            GetComponent<HealthComponent>();

        if (health == null)
            return;

        // Massive guaranteed lethal damage
        health.TakeDamage(99999);
    }

    // =====================================================
    // RESET TEMPORARY REWARD ONLY
    // =====================================================

    public void ResetRewards()
    {
        totalReward = 0f;

        totalHits = 0;

        totalKills = 0;

        totalDamageDealt = 0;
    }
}