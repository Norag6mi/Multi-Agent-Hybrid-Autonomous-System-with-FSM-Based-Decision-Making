using UnityEngine;

/// <summary>
/// Evaluates potential combat positions.
/// Currently returns the agent's current position.
/// Future versions will generate and score multiple candidates.
/// </summary>
public class CombatPositionEvaluator : MonoBehaviour
{
    /// <summary>
    /// Returns the best combat position.
    /// Placeholder implementation.
    /// </summary>
    
    [Header("Candidate Generation")]
    public int candidateCount = 8;

    public float minCandidateDistance = 3f;
    public float maxCandidateDistance = 6f;

    private Vector3[] candidates;

    [Header("Visibility")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float eyeHeight = 1.6f;
    public Vector3 GetBestPosition(Transform target)
    {
        GenerateCandidates(target);

        Vector3 bestPosition = transform.position;
        float bestScore = float.MinValue;

        foreach (Vector3 candidate in candidates)
        {
            float score = ScorePosition(candidate, target);

            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = candidate;
            }
        }

        return bestPosition;
    }

    private void GenerateCandidates(Transform target)
    {
        Debug.Log("Generating candidates");
        if (target == null)
            return;

        candidates = new Vector3[candidateCount];

        for (int i = 0; i < candidateCount; i++)
        {
            float angle =
                Random.Range(0f, 360f);

            float distance =
                Random.Range(
                    minCandidateDistance,
                    maxCandidateDistance);

            Vector3 direction =
                Quaternion.Euler(0, angle, 0) * Vector3.forward;

            candidates[i] =
                target.position +
                direction * distance;
        }
    }

    private float ScorePosition(Vector3 position, Transform target)
    {
        float score = 0f;

        float distance =
            Vector3.Distance(position, target.position);

        //
        // Prefer positions close to preferred distance
        //

        float idealDistance = 10f;

        score -= Mathf.Abs(distance - idealDistance);

        return score;
    }
    private void OnDrawGizmosSelected()
    {
        if (candidates == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (Vector3 point in candidates)
        {
            Gizmos.DrawSphere(point, 0.4f);
        }
    }

    public Vector3 GetBestCoverPosition(Transform target)
    {
        CoverPoint bestCover = GetBestCoverPoint(target);

        if (bestCover == null)
            return transform.position;

        bestCover.Reserve();

        return bestCover.transform.position;
    }


    public CoverPoint GetBestCoverPoint(Transform target)
    {
        CoverPoint[] coverPoints =
            FindObjectsByType<CoverPoint>(FindObjectsSortMode.None);

        CoverPoint bestCover = null;
        float bestScore = float.MinValue;

        foreach (CoverPoint cover in coverPoints)
        {
            if (!cover.IsAvailable())
                continue;

            // Ignore exposed cover
            if (CanEnemySeeCover(cover, target))
                continue;

            float score = ScoreCoverPoint(cover, target);

            if (score > bestScore)
            {
                bestScore = score;
                bestCover = cover;
            }
        }

        return bestCover;
    }

    private float ScoreCoverPoint(CoverPoint cover, Transform target)
    {
        float score = 0f;

        // Prefer cover close to this NPC.
        float distanceToCover =
            Vector3.Distance(
                transform.position,
                cover.transform.position);

        score -= distanceToCover;

        // Prefer cover near the desired combat distance from the enemy.
        float distanceToEnemy =
            Vector3.Distance(
                cover.transform.position,
                target.position);

        const float preferredCombatDistance = 10f;

        score -= Mathf.Abs(distanceToEnemy - preferredCombatDistance);

        return score;
    }


    private bool CanEnemySeeCover(CoverPoint cover, Transform enemy)
    {
        Vector3 enemyEye =
            enemy.position + Vector3.up * eyeHeight;

        Vector3 coverEye =
            cover.transform.position + Vector3.up * eyeHeight;

        bool hasLOS = VisibilityUtility.HasLineOfSight(
            enemyEye,
            coverEye,
            obstacleMask);

            Debug.DrawLine(
            enemyEye,
            coverEye,
            hasLOS ? Color.red : Color.green);

        return hasLOS;
    }
}