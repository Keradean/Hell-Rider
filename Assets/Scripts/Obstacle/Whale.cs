using UnityEngine;
using FishNet.Object;

public class Whale : NetworkBehaviour
{
    private Rigidbody2D rb2d;
    private float baseYVelocity; // to save y direction 0f

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        float worldSpeed = GameManager.Instance != null
            ? GameManager.Instance.worldSpeed
            : -2f;

        baseYVelocity = 0f; 
        rb2d.linearVelocity = new Vector2(worldSpeed, baseYVelocity);
    }

    private void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        float currentWorldSpeed = GameManager.Instance != null
            ? GameManager.Instance.worldSpeed
            : -2f;

        rb2d.linearVelocity = new Vector2(currentWorldSpeed, baseYVelocity);
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (transform.position.x < -11f)
        {
            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }
}