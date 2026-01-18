using FishNet.Object;
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

    [Header("Energy Management")]
    [SerializeField] private float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        energy = maxEnergy;
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
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
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

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        bool canBoost = isBoostPressed && moveInput.x > 0f && energy > 0f;

        boostBackgroundSpeed = canBoost ? boostBackground : 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.worldSpeed = canBoost ? -10f : -2f;
        }

        float currentSpeed = canBoost ? boostSpeed : moveSpeed;
        Vector2 movement = moveInput.normalized * currentSpeed;
        rb.linearVelocity = movement;

        // Energy Management
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

        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);
        animator.SetBool("boosting", isBoosting);
    }
}