using UnityEngine;
using System.Collections.Generic;

public class ExplosionRadiusVisual : MonoBehaviour
{
    [Header("Shockwave")]
    public float maxRadius = 10f;
    public float expandDuration = 3f;

    [Header("Damage")]
    public int explosionDamage = 50;
    public LayerMask damageLayers;

    private float timer;

    private HashSet<HealthComponent> damagedTargets =
        new HashSet<HealthComponent>();

    private void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / expandDuration);

        // Current radius of the expanding shockwave
        float currentRadius =
            Mathf.Lerp(0f, maxRadius, t);

        // Scale sphere (scale uses diameter)
        float currentDiameter =
            currentRadius * 2f;

        transform.localScale =
            Vector3.one * currentDiameter;

        DamageTargets(currentRadius);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void DamageTargets(float currentRadius)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            currentRadius,
            damageLayers);

        foreach (Collider hit in hits)
        {
            HealthComponent health =
                hit.GetComponentInParent<HealthComponent>();

            if (health != null &&
                !damagedTargets.Contains(health))
            {
                damagedTargets.Add(health);

                health.TakeDamage(explosionDamage);

                Debug.Log(
                    $"{health.gameObject.name} was hit by the shockwave.");
            }
        }
    }
}