using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController controller;

    private AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip footstepClip;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Footstep Timing")]
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.32f;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float footstepVolume = 1f;

    [Range(0f, 1f)]
    public float jumpVolume = 1f;

    [Range(0f, 1f)]
    public float landingVolume = 1f;

    private float stepTimer;
    private bool wasGrounded;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<FirstPersonController>();
    }

    private void OnEnable()
    {
        if (controller != null)
            controller.OnJump += PlayJump;
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.OnJump -= PlayJump;
    }

    private void Start()
    {
        wasGrounded = controller.Grounded;
    }

    private void Update()
    {
        HandleFootsteps();
        HandleLanding();

        wasGrounded = controller.Grounded;
    }

    private void HandleFootsteps()
    {
        if (!controller.Grounded)
        {
            stepTimer = 0f;
            return;
        }

        Vector3 velocity = controller.Controller.velocity;
        velocity.y = 0f;

        if (velocity.magnitude < 0.1f)
        {
            stepTimer = 0f;
            return;
        }

        bool sprinting =
            controller.Inputs.sprint &&
            !controller.IsCrouched;

        float interval = sprinting
            ? sprintStepInterval
            : walkStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;

            if (footstepClip != null)
            {
                audioSource.PlayOneShot(footstepClip, footstepVolume);
            }
        }
    }

    private void HandleLanding()
    {
        if (!wasGrounded && controller.Grounded)
        {
            if (landClip != null)
            {
                audioSource.PlayOneShot(landClip, landingVolume);
            }
        }
    }

    private void PlayJump()
    {
        if (jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip, jumpVolume);
        }
    }
}