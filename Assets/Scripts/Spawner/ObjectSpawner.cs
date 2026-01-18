using UnityEngine;
using FishNet.Object;

public class ObjectSpawner : NetworkBehaviour
{
    [Header("Spawning")]
    [SerializeField] private NetworkObject prefab;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnYRange = new Vector2(-5f, 4f);

    private float spawnTimer = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (!IsServerInitialized) return;

        spawnTimer += Time.deltaTime * PlayerController.localInstance.boostBackgroundSpeed; // okey that is not cool with Instance for multiplayer but i do not have really a choice, for the moment.

        if (spawnTimer >= spawnInterval)
        {
            SpawnObject();
            spawnTimer = 0f;
        }
    }

    private void SpawnObject()
    {
        
        float randomY = Random.Range(spawnYRange.x, spawnYRange.y);
        Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);

        
        NetworkObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        Spawn(obj); 
    }
}