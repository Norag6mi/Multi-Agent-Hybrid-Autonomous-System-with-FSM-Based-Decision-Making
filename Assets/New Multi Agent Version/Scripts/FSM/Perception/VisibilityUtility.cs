using UnityEngine;

/// <summary>
/// Shared visibility helper used by all AI systems.
/// </summary>
public static class VisibilityUtility
{
    public static bool HasLineOfSight(
        Vector3 from,
        Vector3 to,
        LayerMask obstacleMask)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return true;

        direction.Normalize();

        return !Physics.Raycast(
            from,
            direction,
            distance,
            obstacleMask);
    }
}