using UnityEngine;
using FishNet.Object;

public class Bullet : NetworkBehaviour
{
    void Update()
    {
        if (!IsServerInitialized) return; 

        transform.position += new Vector3(Weapon.Instance.speed * Time.deltaTime, 0, 0);

        if (transform.position.x > 10f)
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

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
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