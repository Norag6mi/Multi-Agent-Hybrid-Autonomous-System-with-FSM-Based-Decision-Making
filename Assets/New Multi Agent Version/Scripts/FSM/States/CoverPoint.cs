using UnityEngine;

/// <summary>
/// Represents a tactical cover location that can be occupied by one agent.
/// </summary>
public class CoverPoint : MonoBehaviour
{
    [Header("Cover Settings")]
    public bool occupied = false;

    [Tooltip("Direction this cover protects against.")]
    public Vector3 coverNormal = Vector3.forward;

    public bool IsAvailable()
    {
        return !occupied;
    }

    public void Reserve()
    {
        occupied = true;
    }

    public void Release()
    {
        occupied = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = occupied ? Color.red : Color.green;

        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
#endif
}