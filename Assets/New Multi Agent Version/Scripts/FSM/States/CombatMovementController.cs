using UnityEngine;

/// <summary>
/// Controls all combat-related movement.
/// Currently reproduces the existing EngageState behaviour.
/// Future versions will add:
/// - Tactical repositioning
/// - Retreating
/// - Strafing
/// - Flanking
/// - Cover seeking
/// </summary>
[RequireComponent(typeof(NavigationController))]
public class CombatMovementController : MonoBehaviour
{
    private NavigationController navigation;
    private CombatPositionEvaluator evaluator;
    private AgentAnimator agentAnimator;

    private enum MovementState
    {
        Holding,
        Pursuing,
        Retreating,
        Repositioning,
        MovingToCover,
        InCover
    }

    private MovementState currentState = MovementState.Holding;

    [Header("Combat Distance")]
    public float preferredDistance = 10f;

    [Tooltip("How much variation is allowed before moving again.")]
    public float distanceTolerance = 2f;

    private float MinDistance => preferredDistance - distanceTolerance;
    private float MaxDistance => preferredDistance + distanceTolerance;

    [Header("Reposition Timing")]
    public float minRepositionTime = 2f;
    public float maxRepositionTime = 5f;

    [Header("Reposition Distance")]
    public float minRepositionDistance = 3f;
    public float maxRepositionDistance = 6f;

    [Range(20f,180f)]
    public float repositionAngle = 90f;

    private float nextRepositionTime;

    private bool hasDestination;
    private Vector3 destination;

    private bool shouldReposition;

    private CoverPoint currentCover;

    [Header("Cover")]
    public float minCoverDuration = 2f;
    public float maxCoverDuration = 5f;

    private float leaveCoverTime;

    private void Awake()
    {
        navigation = GetComponent<NavigationController>();
        evaluator = GetComponent<CombatPositionEvaluator>();
        agentAnimator = GetComponent<AgentAnimator>();

        if (agentAnimator == null)
        {
            Debug.LogError("AgentAnimator missing.");
        }
        if (evaluator == null)
        {
            Debug.LogError(
                $"CombatPositionEvaluator missing on {gameObject.name}");
        }
        ScheduleNextReposition();
    }

    /// <summary>
    /// Updates movement during combat.
    /// Returns true if the agent is in a position where it can fire.
    /// </summary>
    public bool HandleCombatMovement(
    Transform target,
    float optimalRange,
    float maxRange)
    {
        switch (currentState)
        {
            case MovementState.Pursuing:
                return UpdatePursuing(target, optimalRange);

            case MovementState.Retreating:
                return UpdateRetreating(target);

            case MovementState.Repositioning:
                return UpdateRepositioning(target);
            
            case MovementState.MovingToCover:
                return UpdateMovingToCover(target);

            case MovementState.InCover:
                return UpdateInCover(target);

            default:
                return UpdateHolding(target, optimalRange);
        }
    }

    private bool UpdatePursuing(Transform target, float optimalRange)
    {
        navigation.Pursue(target, preferredDistance);
        navigation.LookAt(target);

        float distance = navigation.DistanceTo(target);

        if (distance <= MaxDistance)
        {
            navigation.Stop();
            currentState = MovementState.Holding;
        }

        return false;
    }
    private bool UpdateHolding(Transform target, float optimalRange)
    {
        navigation.LookAt(target, 10f);

        // Time to choose a new combat position?
        if (Time.time >= nextRepositionTime)
        {
            Vector3 bestPosition = evaluator.GetBestPosition(target);

            navigation.MoveTo(bestPosition, navigation.engageSpeed);

            currentState = MovementState.Repositioning;

            ScheduleNextReposition();

            return false;
        }

        float distance = navigation.DistanceTo(target);

        // Too far -> chase
        if (distance > MaxDistance)
        {
            currentState = MovementState.Pursuing;
            return false;
        }

        // Too close -> retreat
        if (distance < MinDistance)
        {
            currentState = MovementState.Retreating;
            return true;
        }

        if (Random.value < 0.002f)
        {
            CoverPoint cover =
                evaluator.GetBestCoverPoint(target);

            if (cover != null)
            {
                currentCover = cover;
                currentCover.Reserve();

                navigation.MoveTo(
                    currentCover.transform.position,
                    navigation.engageSpeed);

                currentState = MovementState.MovingToCover;

                return false;
            }
        }

        return true;
    }

    private bool UpdateRetreating(Transform target)
    {
        navigation.LookAt(target, 10f);

        Vector3 awayDirection =
            (transform.position - target.position).normalized;

        Vector3 destination =
            transform.position + awayDirection * 3f;

        navigation.MoveTo(destination, navigation.engageSpeed);

        float distance = navigation.DistanceTo(target);

        if (distance >= preferredDistance)
        {
            navigation.Stop();
            currentState = MovementState.Holding;
        }

        return true;
    }

    private bool UpdateRepositioning(Transform target)
    {
        navigation.LookAt(target);

        // Choose a destination only once
        if (!hasDestination)
        {
            destination = evaluator.GetBestPosition(target);

            navigation.MoveTo(destination, navigation.engageSpeed);

            hasDestination = true;
        }

        // Wait until we've arrived
        if (navigation.HasReachedDestination())
        {
            FinishReposition();
        }

        return false;
    }

    private bool UpdateMovingToCover(Transform target)
    {
        navigation.LookAt(target);

        if (navigation.HasReachedDestination())
        {
            Debug.Log($"{name} reached cover!");
            navigation.Stop();
            agentAnimator.EnterCover();

            leaveCoverTime =
                Time.time +
                Random.Range(
                    minCoverDuration,
                    maxCoverDuration);

            currentState = MovementState.InCover;
        }

        return false;
    }

    private bool UpdateInCover(Transform target)
    {
        navigation.LookAt(target);

        if (Time.time >= leaveCoverTime)
        {
            if (currentCover != null)
            {
                
                agentAnimator.ExitCover();
                currentCover.Release();
                currentState = MovementState.Holding;
                currentCover = null;
            }

            currentState = MovementState.Holding;
        }

        return false;
    }

    public void LookAt(Transform target, float speed)
    {
        navigation.LookAt(target, speed);
    }

    public void LookAt(Transform target)
    {
        navigation.LookAt(target);
    }
    public void StopMovement()
    {
        navigation.Stop();
    }

    public void StopImmediate()
    {
        navigation.StopImmediate();
    }

    private void ScheduleNextReposition()
    {
        nextRepositionTime =
            Time.time +
            Random.Range(
                minRepositionTime,
                maxRepositionTime);
    }

    private void FinishReposition()
    {
        navigation.Stop();

        hasDestination = false;
        shouldReposition = false;

        currentState = MovementState.Holding;

        ScheduleNextReposition();
    }

    private Vector3 GetRandomCombatPosition(Transform target)
    {
        Vector3 direction =
            (transform.position - target.position).normalized;

        float angle =
            Random.Range(-repositionAngle, repositionAngle);

        direction =
            Quaternion.Euler(0f, angle, 0f) * direction;

        float distance =
            Random.Range(
                minRepositionDistance,
                maxRepositionDistance);

        return target.position + direction * distance;
    }
}