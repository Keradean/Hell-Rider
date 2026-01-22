using UnityEngine;
using FishNet.Object;

public class BossOne : NetworkBehaviour
{
    private Animator animator;
    private float speedX;
    private float speedY;
    private bool charging;

    private float switchInterval;
    private float switchTimer;

    [SerializeField] private int lives;


    public override void OnStartServer()
    {
        animator = GetComponent<Animator>();
        EnterPatrolState();
    }

    void Update()
    {
        if (!IsServerInitialized) return;

        switchTimer -= Time.deltaTime;

        if (switchTimer <= 0f)
        {
            if (charging)
                EnterPatrolState();
            else
                EnterChargeState();
        }

        if (transform.position.y > 3 || transform.position.y < -3)
            speedY *= -1;

        transform.position += new Vector3(
            speedX * Time.deltaTime,
            speedY * Time.deltaTime,
            0f
        );

        if (transform.position.x <= -11f && IsSpawned)
            ServerManager.Despawn(gameObject);
    }


    void EnterPatrolState()
    {
        speedX = 0;
        speedY = Random.Range(-2f, 2f);
        switchInterval = Random.Range(5f, 10f);
        switchTimer = switchInterval;
        charging = false;
        animator.SetBool("charging", false);
    }

    void EnterChargeState()
    {
        speedX = -5f;
        speedY = 0;
        switchInterval = Random.Range(2f, 2.5f); ;
        switchTimer = switchInterval;
        charging = true;
        animator.SetBool("charging", true);
        AudioManager.Instance.PlayTunedSound(AudioManager.Instance.bossCharge);
    }

    public void TakeDamage(int damage)
    {
        if (charging)
        {
            AudioManager.Instance.PlayTunedSound(AudioManager.Instance.BossHit);
            lives -= damage;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServerInitialized) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(0);
            if (lives <= 0)
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.EnemyDeath2);
                if (IsSpawned)
                    ServerManager.Despawn(gameObject);
            }
        }

    }
}