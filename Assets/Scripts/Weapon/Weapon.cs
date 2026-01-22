using UnityEngine;
using FishNet.Object;

public class Weapon : NetworkBehaviour
{
    public static Weapon Instance;

    [Header("Weapon")]
    [SerializeField] private NetworkObject bulletPrefab;
    public float speed;
    public int damage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Shoot()
    {
        // Nur Owner schießt!
        if (!IsOwner) return; 

        // Server spawnt Bullet!
        ShootServerRpc(transform.position); 
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 position)
    {
        NetworkObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        // FishNet Spawn!
        ServerManager.Spawn(bullet); 
    }
}