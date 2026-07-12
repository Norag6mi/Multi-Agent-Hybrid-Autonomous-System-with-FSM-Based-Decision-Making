using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SurfaceEffect
{
    public string surfaceTag;
    public GameObject vfxPrefab;
    public AudioClip sfxClip;
}

public class SurfaceManager : MonoBehaviour
{
    public static SurfaceManager Instance;

    [Header("Default Impact")]
    public GameObject defaultImpactVFX;
    public AudioClip defaultImpactSFX;

    [Header("Body Impact")]
    public GameObject bodyImpactVFX;
    public AudioClip bodyImpactSFX;

    [Header("General Settings")]
    public float globalImpactScale = 0.03f;

    [Header("Optional Surface Effects")]
    public List<SurfaceEffect> surfaceEffects;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void HandleImpact(GameObject hitObject, Vector3 pos, Vector3 normal, float sizeMultiplier = 1f)
    {
        GameObject vfxToSpawn = defaultImpactVFX;
        AudioClip sfxToPlay = defaultImpactSFX;

        // Body hit
        if (hitObject.CompareTag("Player") || hitObject.CompareTag("Enemy"))
        {
            if (bodyImpactVFX != null)
                vfxToSpawn = bodyImpactVFX;

            if (bodyImpactSFX != null)
                sfxToPlay = bodyImpactSFX;
        }
        else
        {
            // Optional custom surfaces
            foreach (SurfaceEffect effect in surfaceEffects)
            {
                if (hitObject.CompareTag(effect.surfaceTag))
                {
                    if (effect.vfxPrefab != null)
                        vfxToSpawn = effect.vfxPrefab;

                    if (effect.sfxClip != null)
                        sfxToPlay = effect.sfxClip;

                    break;
                }
            }
        }

        // Play Sound
        if (sfxToPlay != null)
        {
            audioSource.PlayOneShot(sfxToPlay);
        }

        // Spawn VFX
        if (vfxToSpawn != null)
        {
            Vector3 spawnPosition = pos + normal * 0.01f;
            Quaternion rotation = Quaternion.LookRotation(normal);

            GameObject impact = Instantiate(vfxToSpawn, spawnPosition, rotation);

            impact.transform.SetParent(hitObject.transform);
            impact.transform.localScale = Vector3.one * globalImpactScale * sizeMultiplier;

            Destroy(impact, 1.5f);
        }
    }

}