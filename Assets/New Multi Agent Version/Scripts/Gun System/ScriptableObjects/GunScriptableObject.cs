using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;

    [Header("Weapon Audio")]
    public AudioClip FireSFX;

    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    private MonoBehaviour activeMonoBehaviour;
    private Transform muzzleTransform;
    private ParticleSystem muzzleFlash;

    private float lastShootTime;
    private ObjectPool<TrailRenderer> trailPool;

    [Header("Damage")]
    public int Damage = 10;

    // Old initialization (AI compatibility)
    public void Initialize(Transform muzzle, MonoBehaviour owner)
    {
        Initialize(muzzle, owner, null);
    }

    // New initialization (Player + muzzle flash)
    public void Initialize(Transform muzzle, MonoBehaviour owner, ParticleSystem flash)
    {
        muzzleTransform = muzzle;
        activeMonoBehaviour = owner;
        muzzleFlash = flash;

        lastShootTime = 0;
        trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
    }

    public void Shoot(Vector3 direction)
    {
        if (muzzleTransform == null)
            return;

        // Fire Rate Check
        if (Time.time < lastShootTime + ShootConfig.FireRate)
            return;

        lastShootTime = Time.time;

        Vector3 shootDirection = direction.normalized
            + new Vector3(
                Random.Range(-ShootConfig.Spread.x, ShootConfig.Spread.x),
                Random.Range(-ShootConfig.Spread.y, ShootConfig.Spread.y),
                Random.Range(-ShootConfig.Spread.z, ShootConfig.Spread.z)
            );

        shootDirection.Normalize();

        Vector3 origin = muzzleTransform.position;

        // Muzzle Flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Gun Sound
        if (FireSFX != null)
        {
            AudioSource.PlayClipAtPoint(FireSFX, origin, 1f);
        }

        if (Physics.Raycast(origin, shootDirection, out RaycastHit hit, float.MaxValue, ShootConfig.HitMask))
        {
            activeMonoBehaviour.StartCoroutine(
                PlayTrail(origin, hit.point, hit)
            );
        }
        else
        {
            activeMonoBehaviour.StartCoroutine(
                PlayTrail(
                    origin,
                    origin + (shootDirection * TrailConfig.MissDistance),
                    new RaycastHit()
                )
            );
        }
    }

    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = trailPool.Get();

        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;

        yield return null;

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                startPoint,
                endPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );

            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        // =====================================================
        // HIT DETECTION
        // =====================================================
        if (hit.collider != null)
        {
            HealthComponent health =
                hit.collider.GetComponentInParent<HealthComponent>();

            if (health != null)
            {
                // CRITICAL FIX: Fallback to non-generic assignment to eliminate CS0411 permanently
                AgentIdentity attacker =
                    activeMonoBehaviour.GetComponent(typeof(AgentIdentity)) as AgentIdentity;

                AgentIdentity victim =
                    health.GetComponentInParent(typeof(AgentIdentity)) as AgentIdentity;

                // =================================================
                // REWARD SYSTEM
                // =================================================
                AgentRewardSystem rewardSystem =
                    activeMonoBehaviour.GetComponent(typeof(AgentRewardSystem)) as AgentRewardSystem;

                // =================================================
                // METRICS
                // =================================================
                if (MetricsLogger.Instance != null && attacker != null)
                {
                    MetricsLogger.Instance.RecordShotHit(
                        attacker.agentName
                    );
                }

                bool wasDeadBefore = health.Model.IsDead;

                // =================================================
                // APPLY DAMAGE
                // =================================================
                health.TakeDamage(Damage);

                // =================================================
                // REWARD HIT
                // =================================================
                if (rewardSystem != null)
                {
                    rewardSystem.RewardHit(Damage);
                }

                // =================================================
                // KILL CHECK
                // =================================================
                if (!wasDeadBefore && health.Model.IsDead)
                {
                    // Metrics
                    if (MetricsLogger.Instance != null &&
                        attacker != null &&
                        victim != null)
                    {
                        MetricsLogger.Instance.RecordKill(
                            attacker.agentName,
                            victim.agentName
                        );
                    }

                    // Reward kill bonus
                    if (rewardSystem != null)
                    {
                        rewardSystem.RewardKill();
                    }
                }
            }

            // =====================================================
            // SURFACE IMPACT
            // =====================================================
            SurfaceManager.Instance.HandleImpact(
                hit.transform.gameObject,
                endPoint,
                hit.normal           
            );
        }

        yield return new WaitForSeconds(TrailConfig.Duration);

        instance.emitting = false;
        instance.gameObject.SetActive(false);

        trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");

        TrailRenderer trail =
            instance.AddComponent<TrailRenderer>();

        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;

        trail.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
}