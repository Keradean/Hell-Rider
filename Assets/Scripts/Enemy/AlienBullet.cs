using UnityEngine;
using FishNet.Object;

public class AlienBullet : NetworkBehaviour
{
    public float speed;

    void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        transform.position += new Vector3(-speed * Time.deltaTime, 0, 0);

        if (transform.position.x < -12f)
        {
            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServerInitialized) return;

        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            PlayHitSoundObserversRpc();

            if (IsSpawned)
            {
                ServerManager.Despawn(gameObject);
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