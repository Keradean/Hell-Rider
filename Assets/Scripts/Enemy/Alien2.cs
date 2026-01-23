using UnityEngine;
using FishNet.Object;
using System.Collections;

public class Alien2 : NetworkBehaviour
{
    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Material defaultMaterial;

    [Header("Materials")]
    [SerializeField] private Material mWhite;

    [Header("Sprites")]
    [SerializeField] private Sprite[] sprites;

    [Header("Movement")]
    private float moveSpeed;
    private Vector3 targetPosition;
    private float moveTimer;
    private float moveInterval;

    [Header("Death")]
    [SerializeField] private NetworkObject alienDeath;
    [SerializeField] private int lives;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            defaultMaterial = spriteRenderer.material;
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        }

        moveSpeed = Random.Range(0.5f, 3f);
        GenerateRandomPosition();
        moveInterval = Random.Range(0.1f, 2f);
        moveTimer = moveInterval;
        transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        if (moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;
        }
        else
        {
            GenerateRandomPosition();
            moveInterval = Random.Range(0.1f, 2f);
            moveTimer = moveInterval;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.position += new Vector3(-2f * Time.deltaTime, 0, 0);

        if (transform.position.x < -11f && IsSpawned)
        {
            ServerManager.Despawn(gameObject);
        }
    }

    private void GenerateRandomPosition()
    {
        float randomX = Random.Range(-4f, 4f);
        float randomY = Random.Range(-4f, 4f);
        targetPosition = new Vector3(randomX, randomY, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServerInitialized) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            lives--;

            FlashWhiteObserversRpc();
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.Hit);

            if (lives <= 0)
            {
                NetworkObject death = Instantiate(alienDeath, transform.position, transform.rotation);
                ServerManager.Spawn(death);
                AudioManager.Instance.PlayTunedSound(AudioManager.Instance.EnemyDeath2);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.alienCounter++;
                    GameManager.Instance.AddAlienKillScore();
                }

                if (IsSpawned)
                {
                    ServerManager.Despawn(gameObject);
                }
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.alienCounter++;
            }

            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }

    [ObserversRpc]
    private void FlashWhiteObserversRpc()
    {
        if (spriteRenderer != null && mWhite != null)
        {
            StartCoroutine(ResetMaterial());
        }
    }
    private IEnumerator ResetMaterial()
    {
        spriteRenderer.material = mWhite;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.material = defaultMaterial;
    }
}