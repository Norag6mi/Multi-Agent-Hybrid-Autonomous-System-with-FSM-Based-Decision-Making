using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class PlayerDeathHandler : MonoBehaviour
{
    private HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.OnDeathEvent += HandleDeath;
    }

    private void HandleDeath()
    {
        Debug.Log("PLAYER DEAD");

        BombMissionManager.Instance?.PlayerDied();

        // Disable movement
        var movement = GetComponent<SimplePlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        // Disable weapon
        var weapon = GetComponent<PlayerWeaponComponent>();
        if (weapon != null)
            weapon.enabled = false;


        // Disable camera
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            cam.enabled = false;

        // Optional: freeze rigidbody
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }
}