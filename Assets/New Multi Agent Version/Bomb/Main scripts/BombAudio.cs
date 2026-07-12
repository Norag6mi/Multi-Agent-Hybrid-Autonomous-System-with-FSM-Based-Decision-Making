using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Bomb))]
[RequireComponent(typeof(BombDefuse))]
public class BombAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource beepSource;
    [SerializeField] private AudioSource actionSource;

    [Header("Audio Clips")]
    public AudioClip beepClip;
    public AudioClip defuseLoopClip;
    public AudioClip defusedClip;
    public AudioClip explosionClip;

    [Header("Beep Settings")]
    public float startBeepInterval = 1.2f;
    public float endBeepInterval = 0.2f;

    [Header("Beep Pitch")]
    public float startPitch = 1f;
    public float endPitch = 1.3f;

    private Bomb bomb;
    private BombDefuse bombDefuse;

    private Coroutine beepRoutine;

    private void Awake()
    {
        bomb = GetComponent<Bomb>();
        bombDefuse = GetComponent<BombDefuse>();
    }

    private void OnEnable()
    {
        bomb.OnExploded += PlayExplosion;
        bomb.OnDefused += PlayDefused;

        bombDefuse.OnDefuseStarted += StartDefuseLoop;
        bombDefuse.OnDefuseCancelled += StopDefuseLoop;
    }

    private void OnDisable()
    {
        bomb.OnExploded -= PlayExplosion;
        bomb.OnDefused -= PlayDefused;

        bombDefuse.OnDefuseStarted -= StartDefuseLoop;
        bombDefuse.OnDefuseCancelled -= StopDefuseLoop;
    }

    private void Start()
    {
        if (beepClip != null)
            beepRoutine = StartCoroutine(BeepRoutine());
    }

    IEnumerator BeepRoutine()
    {
        float startTime = Time.time;
        float nextBeepTime = startTime; 

        // Keeps the first 70% extra slow
        float slowPhaseMultiplier = 1.8f; 
        float extraSlowStartInterval = startBeepInterval * slowPhaseMultiplier;

        while (!bomb.IsExploded && !bomb.IsDefused)
        {
            float currentTime = Time.time;
            float elapsed = currentTime - startTime;
            float t = Mathf.Clamp01(elapsed / bomb.fuseTime);

            if (currentTime >= nextBeepTime)
            {
                float interval;

                // Phase 1: 0% to 70% -> Extra slow pacing
                if (t < 0.7f)
                {
                    interval = extraSlowStartInterval;
                }
                // Phase 2: 70% to 90% -> Accelerating pacing
                else if (t < 0.9f)
                {
                    float phase = (t - 0.7f) / 0.2f; 
                    interval = Mathf.Lerp(extraSlowStartInterval, endBeepInterval * 2f, phase);
                }
                // Phase 3: 90% to 100% -> Frantic, rapid pacing
                else
                {
                    interval = endBeepInterval;
                }

                if (beepSource != null && beepClip != null)
                {
                    // FIX: Pitch is completely locked to startPitch and never changes
                    beepSource.pitch = startPitch;
                    beepSource.PlayOneShot(beepClip);
                }

                nextBeepTime = currentTime + interval;
            }

            yield return null; 

            if (elapsed >= bomb.fuseTime)
                break;
        }

        beepRoutine = null;
    }


    private void StartDefuseLoop()
    {
        if (actionSource == null || defuseLoopClip == null)
            return;

        if (actionSource.isPlaying)
            return;

        actionSource.clip = defuseLoopClip;
        actionSource.loop = true;
        actionSource.Play();
    }

    private void StopDefuseLoop()
    {
        if (actionSource == null)
            return;

        actionSource.Stop();
        actionSource.loop = false;
    }

    private void PlayDefused()
    {
        StopDefuseLoop();

        if (beepRoutine != null)
        {
            StopCoroutine(beepRoutine);
            beepRoutine = null;
        }

        if (beepSource != null)
            beepSource.Stop();

        if (defusedClip != null)
        {
            AudioSource.PlayClipAtPoint(defusedClip, transform.position, 1f);
        }
    }

    private void PlayExplosion()
    {
        StopDefuseLoop();

        if (beepRoutine != null)
        {
            StopCoroutine(beepRoutine);
            beepRoutine = null;
        }

        if (beepSource != null)
            beepSource.Stop();

        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
        }
    }
}