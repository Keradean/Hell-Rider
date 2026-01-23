using UnityEngine;
using FishNet.Object;

public class Weapon : NetworkBehaviour
{
    [Header("Weapon Config")]
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private float speed = 10f;

    [Header("Spread Shot (Rechte Maus)")]
    [SerializeField] private float spreadAngle = 25f;
    [SerializeField] private int maxSpreadShots = 3;
    private int spreadShotCount = 0;

    [Header("Spread Cooldown")]
    [SerializeField] private float spreadResetTime = 5f;
    private float spreadResetTimer = 0f;

    void Update()
    {
        if (!IsOwner) return;

        if (spreadShotCount > 0)
        {
            spreadResetTimer += Time.deltaTime;

            if (spreadResetTimer >= spreadResetTime)
            {
                ResetSpreadShots();
                Debug.Log("Spread Shots aufgeladen!");
            }
        }
    }

    public void Shoot()
    {
        if (!IsOwner) return;

        ShootNormalServerRpc(transform.position, speed);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.Shoot);
        }
    }

    public void ShootSpread()
    {
        if (!IsOwner) return;

        if (spreadShotCount >= maxSpreadShots)
        {
            Debug.Log($"Keine Spread-Schüsse mehr! Warte {spreadResetTime - spreadResetTimer:F1}s");
            return;
        }

        spreadShotCount++;
        spreadResetTimer = 0f; 
        ShootSpreadServerRpc(transform.position, speed);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.Shoot);
        }

        Debug.Log($"Spread Shots: {spreadShotCount}/{maxSpreadShots}");
    }

    public void ResetSpreadShots()
    {
        spreadShotCount = 0;
        spreadResetTimer = 0f;
    }

    [ServerRpc]
    private void ShootNormalServerRpc(Vector3 position, float bulletSpeed)
    {
        SpawnBullet(position, Quaternion.identity, bulletSpeed, Vector3.right);
    }

    [ServerRpc]
    private void ShootSpreadServerRpc(Vector3 position, float bulletSpeed)
    {
        for (int i = -1; i <= 1; i++)
        {
            float angle = i * spreadAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.right;

            SpawnBullet(position, rotation, bulletSpeed, direction);
        }
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, float bulletSpeed, Vector3 direction)
    {
        NetworkObject bullet = Instantiate(bulletPrefab, position, rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.direction = direction.normalized;
        }

        ServerManager.Spawn(bullet);
    }
}