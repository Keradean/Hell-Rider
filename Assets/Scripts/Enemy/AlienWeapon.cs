using UnityEngine;
using FishNet.Object;

public class AlienWeapon : NetworkBehaviour
{
    [Header("Weapon")]
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float fireRate = 1f;

    private float fireTimer;

    void Start()
    {
        fireTimer = Random.Range(0f, 1f / fireRate);
    }

    void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        fireTimer -= Time.fixedDeltaTime;

        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = 1f / fireRate;
        }
    }

    [Server] 
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        NetworkObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

        AlienBullet bulletScript = bullet.GetComponent<AlienBullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = speed;
        }

        ServerManager.Spawn(bullet);

        PlayShootSoundObserversRpc();
    }

    [ObserversRpc]
    private void PlayShootSoundObserversRpc()
    {
        if (AudioManager.Instance != null)
        {
            // AudioManager.Instance.PlaySound(AudioManager.Instance.AlienShoot);
        }
    }
}