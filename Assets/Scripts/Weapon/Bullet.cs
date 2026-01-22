using UnityEngine;
using FishNet.Object;

public class Bullet : NetworkBehaviour
{
    void Update()
    {
        if (!IsServerInitialized) return; // ← Nur Server bewegt!

        transform.position += new Vector3(Weapon.Instance.speed * Time.deltaTime, 0, 0);

        if (transform.position.x > 10f)
        {
            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject); // ← FishNet Despawn!
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServerInitialized) return; // ← Nur Server prüft!

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
        {
            PlayHitSoundObserversRpc(); // ← Sound für alle!

            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject); // ← FishNet Despawn!
            }
        }
    }

    [ObserversRpc]
    private void PlayHitSoundObserversRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.Hit);
        }
    }
}