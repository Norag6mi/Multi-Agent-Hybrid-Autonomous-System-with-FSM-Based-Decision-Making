using UnityEngine;

[RequireComponent(typeof(AgentRewardSystem))]
public class ObstaclePenalty : MonoBehaviour
{
    [Header("Obstacle Detection")]
    public LayerMask obstacleMask;

    [Header("Penalty Settings")]
    public float penaltyCooldown = 1f;

    private float lastPenaltyTime = -999f;

    private AgentRewardSystem rewardSystem;

    private void Awake()
    {
        rewardSystem =
            GetComponent<AgentRewardSystem>();
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if collided object belongs
        // to any layer in obstacleMask

        if ((obstacleMask.value &
            (1 << collision.gameObject.layer)) == 0)
        {
            return;
        }

        // Cooldown so agent doesn't lose
        // hundreds of points per second

        if (Time.time - lastPenaltyTime <
            penaltyCooldown)
        {
            return;
        }

        lastPenaltyTime = Time.time;

        rewardSystem.PenalizeObstacleCollision();

        Debug.Log(
            $"[OBSTACLE PENALTY] " +
            $"{gameObject.name} collided with " +
            $"{collision.gameObject.name}"
        );
    }
}