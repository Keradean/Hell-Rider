using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Singelton")]
    public static PlayerMovement localInstance;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            localInstance = this;
            GetComponent<PlayerInput>().enabled = true;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

    }
    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.performed)
            isBoostPressed = true;
        else if (context.canceled)
            isBoostPressed = false;
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Only allow boosting when moving right
        bool canBoost = isBoostPressed && moveInput.x > 0f;
        boostBackgroundSpeed = canBoost ? boostBackground : 1f;

        float currentSpeed = canBoost ? boostSpeed : moveSpeed;
        Vector2 movement = moveInput.normalized * currentSpeed;
        rb.linearVelocity = movement;

        UpdateAnimator(canBoost);
    }

    private void UpdateAnimator(bool isBoosting)
    {
        if (animator == null) return;

        // Blend Tree Parameters = Movement X and Y
        animator.SetFloat("moveX", moveInput.x);
        animator.SetFloat("moveY", moveInput.y);

        // Boost Parameter
        animator.SetBool("boosting", isBoosting);
    }

}