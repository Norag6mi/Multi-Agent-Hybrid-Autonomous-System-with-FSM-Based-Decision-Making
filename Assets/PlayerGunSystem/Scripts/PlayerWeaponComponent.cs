using UnityEngine;

public class PlayerWeaponComponent : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float reloadTime = 2f;

    [Header("Gun")]
    [SerializeField] private GunScriptableObject gun;
    [SerializeField] private Transform muzzlePoint;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("State")]
    public bool CanUseWeapon = true;

    private WeaponModel model;
    private bool isFiring;

    private void Awake()
    {
        model = new WeaponModel(magazineSize, fireRate, reloadTime);
    }

    private void Start()
    {
        gun = Instantiate(gun);

        gun.Initialize(muzzlePoint, this, muzzleFlash);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!CanUseWeapon)
        {
            isFiring = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isFiring = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isFiring = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }

        if (isFiring)
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (!model.CanShoot())
            return;

        model.ConsumeAmmo();

        Vector3 direction = muzzlePoint.forward;
        gun.Shoot(direction);
    }

    private void TryReload()
    {
        if (model.IsReloading)
            return;

        model.StartReload();
        StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(reloadTime);
        model.FinishReload();
    }

    public int GetCurrentAmmo()
    {
        return model.CurrentAmmo;
    }
}