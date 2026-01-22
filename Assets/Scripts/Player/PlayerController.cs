using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameKit.Dependencies.Utilities.ObjectPooling.Examples;
using System.Collections;
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
    private SpriteRenderer spriteRenderer;

    private Material defaultMaterial;
    [SerializeField] private Material mWhite;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;
        float randomScale = Random.Range(0.6f, 1f);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

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
                AudioManager.Instance.PlaySound(AudioManager.Instance.Boost);
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
                    AudioManager.Instance.PlaySound(AudioManager.Instance.Pause);
                }
            }
            else
            {
                UIController.Instance.pausePannel.SetActive(false);
                if (IsOwner) 
                { 
                Time.timeScale = 1f;
                AudioManager.Instance.PlaySound(AudioManager.Instance.Unpause);
                }
            }
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isPausedLocally) return;

            Weapon.Instance.Shoot();
        }
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

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
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
            AudioManager.Instance.PlaySound(AudioManager.Instance.Hit);
            spriteRenderer.material = mWhite;
            StartCoroutine(ResetMaterial());
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
    IEnumerator ResetMaterial()
    {
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.material = defaultMaterial;
    }


    [ServerRpc]
    private void DieServerRpc()
    {
        // Explosion über Netzwerk spawnen
        if (explosionEffect != null)
        {
            NetworkObject explosion = Instantiate(explosionEffect, transform.position, transform.rotation);
            ServerManager.Spawn(explosion);
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.Death);
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