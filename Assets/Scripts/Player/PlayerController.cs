using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameKit.Dependencies.Utilities.ObjectPooling.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Singleton")]
    public static PlayerController localInstance;

    [Header("Config")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float boostSpeed;

    [Header("Components")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool isBoostPressed;

    [Header("Background")]
    public float boostBackgroundSpeed = 1f;
    [SerializeField] private float boostBackground = 5f;

    [Header("Health Management")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private NetworkObject explosionEffect;

    [Header("Energy Management")]
    [SerializeField] private float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegen;

    [Header("Pause")]
    private bool isPausedLocally = false;

    [Header("Shooting")]
    [SerializeField] private Transform weapon;
   // private BulletSpawner bulletSpawner;

    private readonly SyncVar<Vector2> syncVelocity = new SyncVar<Vector2>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        energy = maxEnergy;
        health = maxHealth;

      //  bulletSpawner = FindFirstObjectByType<BulletSpawne>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            localInstance = this;
            GetComponent<PlayerInput>().enabled = true;
            if (UIController.Instance != null)
            {
                UIController.Instance.UpdateEnegeryBar(energy, maxEnergy);
                UIController.Instance.UpdateHealthBar(health, maxHealth);
            }
        }
        else
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isPausedLocally) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (isPausedLocally) return;

        if (context.performed)
        {
            if (energy > 10)
            {
                isBoostPressed = true;
            }
        }
        else if (context.canceled)
        {
            isBoostPressed = false;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPausedLocally = !isPausedLocally;

        if (UIController.Instance != null)
        {
            if (isPausedLocally)
            {
                UIController.Instance.pausePannel.SetActive(true);
                if (IsOwner)
                {
                    Time.timeScale = 0f;
                }
            }
            else
            {
                UIController.Instance.pausePannel.SetActive(false);
                if (IsOwner) 
                { 
                Time.timeScale = 1f;
                }
            }
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isPausedLocally) return;

            Shoot();
        }
    }

    private void Shoot()
    {
       // if (bulletSpawner == null) return;

        // Schussrichtung nach rechts
        Vector2 shootDirection = Vector2.right;

        // Spawn Position weapon
        Vector3 spawnPos = weapon != null ? weapon.position : transform.position;

        // Spawner wird aufgerufen
        //bulletSpawner.SpawnProjectileServerRpc(spawnPos, shootDirection);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Pause Check
        if (isPausedLocally)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool canBoost = isBoostPressed && moveInput.x > 0f && energy > 0f;
        boostBackgroundSpeed = canBoost ? boostBackground : 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.worldSpeed = canBoost ? -10f : -2f;
        }

        float currentSpeed = canBoost ? boostSpeed : moveSpeed;
        Vector2 movement = moveInput.normalized * currentSpeed;
        rb.linearVelocity = movement;

        syncVelocity.Value = movement;

        if (canBoost && energy > 0f)
        {
            energy -= 0.2f;
            if (energy < 0f) energy = 0f;
        }
        else if (!canBoost && energy < maxEnergy)
        {
            energy += energyRegen;
            if (energy > maxEnergy) energy = maxEnergy;
        }

        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateEnegeryBar(energy, maxEnergy);
        }

        UpdateAnimator(canBoost);
    }

    private void UpdateAnimator(bool isBoosting)
    {
        if (animator == null) return;

        // Lokale Animation setzen
        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);
        animator.SetBool("boosting", isBoosting);

        // Über RPC synchronisieren
        if (IsOwner)
        {
            UpdateAnimationObserversRpc(moveInput.x, moveInput.y, isBoosting);
        }
    }

    [ObserversRpc(ExcludeOwner = true, BufferLast = true)]
    private void UpdateAnimationObserversRpc(float moveX, float moveY, bool boosting)
    {
        if (animator != null)
        {
            animator.SetFloat("moveX", moveX);
            animator.SetFloat("moveY", moveY);
            animator.SetBool("boosting", boosting);
        }
    }

    // Für nicht Owner: Update Position basierend auf Velocity
    private void Update()
    {
        if (IsOwner) return;

        // Position wird geupdaten
        if (rb != null)
        {
            transform.position += (Vector3)syncVelocity.Value * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;
        if (isPausedLocally) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(1);
        }
    }

    // Schaden nehmen und Ui aktualisieren
    private void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;

        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateHealthBar(health, maxHealth);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DieServerRpc();
        moveSpeed = 0f;
        boostSpeed = 0f;

        if (IsOwner && GameOverManager.Instance != null)
        {
            GameOverManager.Instance.LoadGameOverDelayed(2f);
        }
    }


    [ServerRpc]
    private void DieServerRpc()
    {
        // Explosion über Netzwerk spawnen
        if (explosionEffect != null)
        {
            NetworkObject explosion = Instantiate(explosionEffect, transform.position, transform.rotation);
            ServerManager.Spawn(explosion);
        }

        // Player wird deaktivieren
        DieObserversRpc();
    }

    [ObserversRpc]
    private void DieObserversRpc()
    {
        gameObject.SetActive(false);
    }
  
}