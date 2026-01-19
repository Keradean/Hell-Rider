using UnityEngine;
using FishNet.Object;

public class Asteroid : NetworkBehaviour
{
    [Header("Asteroid Sprites")]
    [SerializeField] private Sprite[] sprites;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2d;
    private float pushY; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();

        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

       
        if (transform.position.x < -11)
        {
            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        // Update Velocity mit aktuellem worldSpeed
        float currentWorldSpeed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : -2f;

        rb2d.linearVelocity = new Vector2(currentWorldSpeed, pushY);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        int randomIndex = Random.Range(0, sprites.Length);
        SetSpriteClientRpc(randomIndex);

        
        pushY = Random.Range(-1f, 1f);

        float worldSpeed = GameManager.Instance != null
            ? GameManager.Instance.worldSpeed
            : -2f;

        rb2d.linearVelocity = new Vector2(worldSpeed, pushY);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetSpriteClientRpc(int spriteIndex)
    {
        if (sprites.Length > 0 && spriteIndex < sprites.Length)
        {
            spriteRenderer.sprite = sprites[spriteIndex];
        }
    }
}