using System;
using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [Header("Explosion")]
    public float fuseTime = 30f;
    public float explosionRadius = 10f;
    public int explosionDamage = 50;

    [Header("Layers")]
    public LayerMask damageLayers;

    [Header("Visuals")]
    public GameObject explosionVisualPrefab;

    private Coroutine fuseRoutine;

    private bool exploded = false;
    private bool defused = false;

    public bool IsExploded => exploded;
    public bool IsDefused => defused;

    public event Action OnExploded;
    public event Action OnDefused;

    private void Start()
    {
        fuseRoutine = StartCoroutine(FuseTimer());
    }

    private IEnumerator FuseTimer()
    {
        yield return new WaitForSeconds(fuseTime);

        if (!defused)
            Explode();
    }

    public void Explode()
    {
        if (exploded || defused)
            return;

        exploded = true;

        GameObject visual = Instantiate(
            explosionVisualPrefab,
            transform.position,
            Quaternion.identity);

        ExplosionRadiusVisual effect =
            visual.GetComponent<ExplosionRadiusVisual>();

        if (effect != null)
        {
            effect.maxRadius = explosionRadius;
            effect.explosionDamage = explosionDamage;
            effect.damageLayers = damageLayers;
        }

        OnExploded?.Invoke();

        Debug.Log("BOOM!");

        Destroy(gameObject);
    }

    public void Defuse()
    {
        if (exploded || defused)
            return;

        defused = true;

        if (fuseRoutine != null)
            StopCoroutine(fuseRoutine);

        OnDefused?.Invoke();

        Debug.Log("Bomb Defused!");

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}