using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterAnimatorBridge))]
public class WeaponComponent : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float reloadTime = 2f;

    [Header("Gun System")]
    [SerializeField] private GunScriptableObject gun;
    [SerializeField] private Transform muzzlePoint;

    private bool isFiring;

    public WeaponModel Model { get; private set; }

    private CharacterAnimatorBridge animatorBridge;

    private void Awake()
    {
        animatorBridge = GetComponent<CharacterAnimatorBridge>();
        Model = new WeaponModel(magazineSize, fireRate, reloadTime);
    }

    private void Start()
    {
        if (gun != null)
        {
            gun = Instantiate(gun);
            gun.Initialize(muzzlePoint, this);
        }
    }

    // --- Firing control ---
    public void TryStartFiring()
    {
        if (!Model.CanShoot()) return;

        isFiring = true;
        animatorBridge.SetFiring(true);
    }

    public void StopFiring()
    {
        isFiring = false;
        animatorBridge.SetFiring(false);
    }

    // Updated Fire method using AwarenessModel target
    public void Fire()
    {
        if (!isFiring || !Model.CanShoot()) return;

        // Get current target from AwarenessModel
        AwarenessModel awareness = GetComponent<AwarenessModel>();
        Transform target = (awareness != null) ? awareness.currentTarget : null;

        if (target != null)
        {
            FireAtTarget(target);
        }
        else
        {
            // Optional: fire straight forward if no target
            Vector3 direction = muzzlePoint.forward;
            gun.Shoot(direction);
        }
    }

    // Shoot at a specific target (handles vertical aiming)
    public void FireAtTarget(Transform target)
    {
        if (!isFiring || !Model.CanShoot() || target == null) return;

        Model.ConsumeAmmo();

        // Metrics logging
        AgentIdentity identity = GetComponent<AgentIdentity>();
        if (MetricsLogger.Instance != null && identity != null)
            MetricsLogger.Instance.RecordShotFired(identity.agentName);

        // Calculate shooting direction toward target (aim for chest/head)
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 direction = (targetPos - muzzlePoint.position).normalized;

        gun.Shoot(direction);
    }

    // --- Reload ---
    public void TryReload()
    {
        if (Model.IsReloading) return;

        Model.StartReload();
        animatorBridge.PlayReload();
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(reloadTime);
        Model.FinishReload();
    }

    public void ResetWeapon()
    {
        StopAllCoroutines();
        isFiring = false;

        if (Model != null)
            Model.ResetAmmo();

        if (animatorBridge != null)
            animatorBridge.SetFiring(false);
    }
}