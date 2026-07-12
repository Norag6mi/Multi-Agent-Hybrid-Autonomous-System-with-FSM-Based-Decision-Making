using System;
using UnityEngine;

[RequireComponent(typeof(Bomb))]
[RequireComponent(typeof(SphereCollider))]
public class BombDefuse : MonoBehaviour
{
    [Header("Defuse Settings")]
    public float defuseTime = 5f;

    private Bomb bomb;

    private bool playerInRange;
    private float progress;

    private PlayerGun playerGun;
    private bool gunHiddenByDefuse;
    
    public event Action OnDefuseStarted;
    public event Action OnDefuseCancelled;

    private bool wasDefusing = false;

    private PlayerWeaponComponent playerWeapon;

    private void Awake()
    {
        bomb = GetComponent<Bomb>();

        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
    }

    private void Start()
    {
        DefuseUIManager.Instance?.Hide();
    }

    private void Update()
    {
        if (bomb.IsExploded || bomb.IsDefused)
            return;

        if (!playerInRange)
            return;

        if (Input.GetKey(KeyCode.E))
        {
            if (!wasDefusing)
            {
                wasDefusing = true;
                OnDefuseStarted?.Invoke();
            }

            HideGun();

            progress += Time.deltaTime;

            DefuseUIManager.Instance?.SetProgress(progress / defuseTime);

            if (progress >= defuseTime)
            {
                BombInteractionManager.Instance.ReleaseInteraction(this);

                bomb.Defuse();

                RestoreGun();

                DefuseUIManager.Instance?.Hide();
            }
        }
        else
        {
            progress = 0f;

            DefuseUIManager.Instance?.SetProgress(0);

            if (wasDefusing)
            {
                wasDefusing = false;
                OnDefuseCancelled?.Invoke();
            }

            RestoreGun();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player2"))
            return;

        if (!BombInteractionManager.Instance.RequestInteraction(this))
            return;

        playerInRange = true;

        // Find the marker component anywhere under the player.
        playerGun = other.GetComponentInChildren<PlayerGun>();
        playerWeapon = other.GetComponent<PlayerWeaponComponent>();

        Debug.Log("Player entered bomb range.");

        DefuseUIManager.Instance?.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player2"))
            return;

        BombInteractionManager.Instance.ReleaseInteraction(this);
        playerInRange = false;

        progress = 0f;

        if (wasDefusing)
        {
            wasDefusing = false;
            OnDefuseCancelled?.Invoke();
        }

        RestoreGun();

        DefuseUIManager.Instance?.Hide();

        Debug.Log("Player left bomb range.");
    }

    private void HideGun()
    {
        if (playerGun != null && !gunHiddenByDefuse)
        {
            playerGun.gameObject.SetActive(false);
            gunHiddenByDefuse = true;
        }

        if (playerWeapon != null)
        {
            playerWeapon.CanUseWeapon = false;
        }
    }

    private void RestoreGun()
    {
        if (playerGun != null && gunHiddenByDefuse)
        {
            playerGun.gameObject.SetActive(true);
            gunHiddenByDefuse = false;
        }

        if (playerWeapon != null)
        {
            playerWeapon.CanUseWeapon = true;
        }
    }
}