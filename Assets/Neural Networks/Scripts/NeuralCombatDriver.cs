using UnityEngine;

/// <summary>
/// Neural combat processor.
/// Handles:
/// - Neural inputs (Enemy vectors, normalized distance, and 8-directional wall sensors)
/// - Obstacle sensing (360-degree local space awareness)
/// - Neural outputs
/// - Movement
/// - Shooting
/// </summary>

public class NeuralCombatDriver : MonoBehaviour
{
    [Header("References")]
    public CopyNeuralNetwork nn;
    public AgentFSM fsm;

    [Header("Obstacle Detection")]
    public LayerMask obstacleMask;

    [Header("Ray Sensors")]
    public float sensorRange = 10f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("NN Outputs")]
    public float FB;
    public float LR;
    public float Shoot;

    private Transform currentTarget;

    private void Awake()
    {
        if (nn == null)
            nn = GetComponent<CopyNeuralNetwork>();

        if (fsm == null)
            fsm = GetComponent<AgentFSM>();
    }

    void Update()
    {
        // ======================================
        // SAFETY CHECKS
        // ======================================

        if (nn == null || fsm == null)
            return;

        if (fsm.CurrentStateKey != AgentState.Engage)
            return;

        if (fsm.Identity == null ||
            fsm.Identity.Combat == null)
            return;

        IAgentCombat combat =
            fsm.Identity.Combat;

        if (combat.IsDead())
            return;

        // ======================================
        // TARGET
        // ======================================

        currentTarget =
            fsm.AwarenessModel.currentTarget;

        if (currentTarget == null)
        {
            combat.StopAttack();
            return;
        }

        // ======================================
        // ENEMY INPUTS
        // LOCAL SPACE NORMALIZED DIRECTION
        // ======================================

        Vector3 enemyDir =
            (currentTarget.position - transform.position).normalized;

        Vector3 localEnemyDir =
            transform.InverseTransformDirection(enemyDir);

        float enemyX = localEnemyDir.x;
        float enemyZ = localEnemyDir.z;

        // ======================================
        // ENEMY DISTANCE INPUT (Normalized by 20m max)
        // ======================================

        float enemyDistance =
            Mathf.Clamp01(
                Vector3.Distance(
                    transform.position,
                    currentTarget.position
                ) / 20f
            );

        // ======================================
        // 8 DIRECTION LOCAL RAY SENSORS
        // ======================================

        float[] rays = new float[8];

        Vector3 eyePos =
            transform.position + Vector3.up * 1.5f;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;

            // Guaranteed to stay relative and rotate seamlessly with the agent
            Vector3 rayDirection =
                transform.TransformDirection(
                    Quaternion.Euler(0f, angle, 0f) * Vector3.forward
                );

            RaycastHit hit;

            if (Physics.Raycast(
                eyePos,
                rayDirection,
                out hit,
                sensorRange,
                obstacleMask))
            {
                // Wall detected: return normalized distance (0 = touching wall, 1 = clear)
                rays[i] = hit.distance / sensorRange;
            }
            else
            {
                // Path clear
                rays[i] = 1f;
            }

            // Visual diagnostic lines inside the Scene view
            Debug.DrawRay(
                eyePos,
                rayDirection * (rays[i] * sensorRange),
                Color.green
            );
        }

        // ======================================
        // INPUTS TO NEURAL NETWORK (11 total)
        // ======================================

        float[] inputsToNN =
        {
            enemyX,
            enemyZ,
            enemyDistance,

            rays[0], // Front
            rays[1], // Front Right
            rays[2], // Right
            rays[3], // Back Right
            rays[4], // Back
            rays[5], // Back Left
            rays[6], // Left
            rays[7]  // Front Left
        };

        // ======================================
        // OUTPUTS FROM NEURAL NETWORK
        // ======================================

        float[] outputs =
            nn.Brain(inputsToNN);

        FB = outputs[0];
        LR = outputs[1];
        Shoot = outputs[2];

        // ======================================
        // MOVEMENT
        // ======================================

        Move(FB, LR);

        // ======================================
        // SHOOTING
        // ======================================

        HandleShooting(combat);

        // ======================================
        // DEBUG
        // ======================================

        Debug.Log(
            $"EnemyX:{enemyX:F2} | EnemyZ:{enemyZ:F2} | Dist:{enemyDistance:F2} || " +
            $"F:{rays[0]:F1} R:{rays[2]:F1} B:{rays[4]:F1} L:{rays[6]:F1} || " +
            $"FB:{FB:F2} | LR:{LR:F2} | Shoot:{Shoot:F2}"
        );
    }

    // =====================================================
    // MOVEMENT
    // =====================================================

    private void Move(float FB, float LR)
    {
        if (currentTarget == null)
            return;

        // Clamp outputs
        FB = Mathf.Clamp(FB, -1f, 1f);
        LR = Mathf.Clamp(LR, -1f, 1f);

        // ======================================
        // MOVEMENT THRESHOLD
        // ======================================

        float moveThreshold = 0.3f;

        if (Mathf.Abs(FB) < moveThreshold)
            FB = 0f;

        if (Mathf.Abs(LR) < moveThreshold)
            LR = 0f;

        // Face target
        fsm.Navigation.LookAt(currentTarget, 20f);

        // Local movement direction
        Vector3 moveDir =
            (transform.forward * FB) +
            (transform.right * LR);

        moveDir.y = 0f;

        // Small deadzone
        if (moveDir.magnitude < 0.1f)
            return;

        moveDir.Normalize();

        // Move agent
        transform.position +=
            moveDir * moveSpeed * Time.deltaTime;
    }

    // =====================================================
    // SHOOTING
    // =====================================================

    private void HandleShooting(IAgentCombat combat)
    {
        if (currentTarget == null)
            return;

        float dist =
            fsm.Navigation.DistanceTo(currentTarget);

        // Too far
        if (dist > fsm.maxEngageRange * 2f)
        {
            combat.StopAttack();
            return;
        }

        // Reloading
        if (combat.IsReloading())
            return;

        // Empty ammo
        if (combat.GetCurrentAmmo() <= 0)
        {
            combat.StopAttack();
            combat.Reload();
            return;
        }

        // Neural shoot decision
        if (Shoot > 0.5f)
        {
            combat.StartAttack();
        }
        else
        {
            combat.StopAttack();
        }
    }
}