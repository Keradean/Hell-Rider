using UnityEngine;
using FishNet.Object;

public class BossOne : NetworkBehaviour
{
    [Header("Movement")]
    private float speedX;
    private float speedY;
    private bool charging;

    [Header("State Timer")]
    private float switchInterval;
    private float switchTimer;

    [Header("References")]
    private Animator animator;
    private Transform playerTransform;

    [Header("Stats")]
    [SerializeField] private int lives = 10;

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindPlayer();
        EnterPatrolState();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        animator = GetComponent<Animator>();
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;

        if (PlayerController.localInstance != null)
        {
            playerTransform = PlayerController.localInstance.transform;
            Debug.Log("Boss found player via localInstance");
        }
        else
        {
            // Suche nach allen Spielern im Netzwerk
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (players.Length > 0)
            {
                playerTransform = players[0].transform;
            }

        }
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized)
            return;

        switchTimer -= Time.fixedDeltaTime;
        if (switchTimer <= 0f)
        {
            if (charging)
                EnterPatrolState();
            else
                EnterChargeState();
        }

        float playerX = playerTransform.position.x;
        float playerY = playerTransform.position.y;

        if (transform.position.y > 3f || transform.position.y < -3f)
        {
            speedY *= -1;
        }
        else if (!charging && transform.position.x < playerX)
        {
            EnterChargeState();
            speedY = Mathf.Sign(playerY - transform.position.y) * 2f;
        }

        transform.position += new Vector3(
            speedX * Time.fixedDeltaTime,
            speedY * Time.fixedDeltaTime,
            0f
        );

        if (transform.position.x <= -11f && IsSpawned)
        {
            ServerManager.Despawn(gameObject);
        }
    }

    void EnterPatrolState()
    {
        speedX = 0f;
        speedY = Random.Range(-2f, 2f);
        switchInterval = Random.Range(5f, 10f);
        switchTimer = switchInterval;
        charging = false;
        RpcSetCharging(false);
    }

    void EnterChargeState()
    {
        speedX = -5f;
        speedY = 0f;
        switchInterval = Random.Range(2f, 2.5f);
        switchTimer = switchInterval;
        charging = true;
        RpcSetCharging(true);
        RpcPlayChargeSound();
    }

    [Server]
    public void TakeDamage(int damage)
    {
        lives -= damage;
        RpcPlayHitSound();

        if (lives <= 0)
        {
            RpcPlayDeathSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddBossKillScore();
            }

            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServerInitialized)
            return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);
        }
    }

    [ObserversRpc]
    void RpcSetCharging(bool value)
    {
        if (animator != null)
            animator.SetBool("charging", value);
    }

    [ObserversRpc]
    void RpcPlayChargeSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.bossCharge);
        }
    }

    [ObserversRpc]
    void RpcPlayHitSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.BossHit);
        }
    }

    [ObserversRpc]
    void RpcPlayDeathSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.EnemyDeath2);
        }
    }
}