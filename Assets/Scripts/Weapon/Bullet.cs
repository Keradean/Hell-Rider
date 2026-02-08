using FishNet.Object;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed;
    public Vector3 direction = Vector3.right;

    void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        transform.position += direction * speed * Time.fixedDeltaTime;

        if (transform.position.x > 10f || transform.position.x < -10f ||
            transform.position.y > 10f || transform.position.y < -10f)
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

        if (collision.gameObject.CompareTag("Obstacle") ||
            collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Boss"))
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