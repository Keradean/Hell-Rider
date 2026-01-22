using UnityEngine;
using FishNet.Object;

public class Alien1 : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    private float moveSpeed;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float moveTimer;
    private float moveInterval;
    [SerializeField] private NetworkObject alienDeath; 
    [SerializeField] private NetworkObject alienBurn; 
    [SerializeField] private int lives;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        moveSpeed = Random.Range(0.5f, 3f);
        GenerateRandomPosition();
        moveInterval = Random.Range(0.1f, 2f);
        moveTimer = moveInterval;
    }

    void Update()
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

        Vector3 relativePosition = targetPosition - transform.position;
        if (relativePosition != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(Vector3.forward, relativePosition);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 1080 * Time.deltaTime);
        }

        float moveX = (GameManager.Instance.worldSpeed) * Time.deltaTime;
        transform.position += new Vector3(moveX, 0);

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
            if (lives <= 0)
            {
                NetworkObject death = Instantiate(alienDeath, transform.position, transform.rotation);
                ServerManager.Spawn(death);
                AudioManager.Instance.PlayTunedSound(AudioManager.Instance.EnemyDeath2);

                if (IsSpawned)
                {
                    ServerManager.Despawn(gameObject);
                }
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            NetworkObject burn = Instantiate(alienBurn, transform.position, transform.rotation);
            ServerManager.Spawn(burn);
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.Burn);

            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }
}