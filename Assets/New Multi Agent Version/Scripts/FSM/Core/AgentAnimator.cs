using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class AgentAnimator : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    // Animator Parameter Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsInCoverHash = Animator.StringToHash("IsInCover");

    [Header("Smoothing")]
    [SerializeField] private float speedSmoothing = 0.1f;
    [SerializeField] private float directionSmoothing = 0.1f;

    private float smoothedSpeed;
    private float smoothedMoveX;
    private float smoothedMoveY;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        agent = GetComponentInParent<NavMeshAgent>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        UpdateLocomotion();
        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetBool("IsInCover", true);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetBool("IsInCover", false);
        }
    }

    private void UpdateLocomotion()
    {
        if (agent == null || animator == null)
            return;

        if (!agent.enabled)
        {
            StopAll();
            return;
        }

        // World-space velocity
        Vector3 worldVelocity = agent.velocity;

        // Convert to local-space velocity
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        // Speed
        float currentSpeed = worldVelocity.magnitude;

        smoothedSpeed = Mathf.Lerp(
            smoothedSpeed,
            currentSpeed,
            Time.deltaTime / speedSmoothing);

        smoothedMoveX = Mathf.Lerp(
            smoothedMoveX,
            localVelocity.x,
            Time.deltaTime / directionSmoothing);

        smoothedMoveY = Mathf.Lerp(
            smoothedMoveY,
            localVelocity.z,
            Time.deltaTime / directionSmoothing);

        animator.SetFloat(SpeedHash, smoothedSpeed);
        animator.SetFloat(MoveXHash, smoothedMoveX);
        animator.SetFloat(MoveYHash, smoothedMoveY);
    }

    public void StopAll()
    {
        smoothedSpeed = 0f;
        smoothedMoveX = 0f;
        smoothedMoveY = 0f;

        animator.SetFloat(SpeedHash, 0f);
        animator.SetFloat(MoveXHash, 0f);
        animator.SetFloat(MoveYHash, 0f);
    }

    public void EnterCover()
    {
        Debug.Log($"{name} ENTER COVER");
        animator.SetBool(IsInCoverHash, true);
    }

    public void ExitCover()
    {
        animator.SetBool(IsInCoverHash, false);
    }
}