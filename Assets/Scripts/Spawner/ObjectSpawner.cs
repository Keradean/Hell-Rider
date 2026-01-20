using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;

public class ObjectSpawner : NetworkBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnYRange = new Vector2(-5f, 4f);

    [Header("Wave Spawning")]
    [SerializeField] private int waveNumber;
    [SerializeField] private List<Wave> waves;

    [System.Serializable]
    public class Wave
    {
        public NetworkObject prefab;
        public float spawnTimer;
        public float spawnInterval;
        public int objectsPerWave;
        public int SpawnedObjectCount;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (waves.Count > 0)
        {
            waves[waveNumber].spawnTimer = waves[waveNumber].spawnInterval;
        }
    }

    void Update()
    {
        if (!IsServerInitialized) return;
        if (waves.Count == 0) return;

        float speedMultiplier = 1f;

        if (GameManager.Instance != null)
        {
            float currentWorldSpeed = GameManager.Instance.worldSpeed;
            speedMultiplier = currentWorldSpeed <= -10f ? 5f : 1f; 
        }

        waves[waveNumber].spawnTimer += Time.deltaTime * speedMultiplier;

        if (waves[waveNumber].spawnTimer >= waves[waveNumber].spawnInterval)
        {
            SpawnObject();
            waves[waveNumber].spawnTimer = 0;
        }

        if (waves[waveNumber].SpawnedObjectCount >= waves[waveNumber].objectsPerWave)
        {
            waves[waveNumber].SpawnedObjectCount = 0;
            waveNumber++;

            if (waveNumber >= waves.Count)
            {
                waveNumber = 0;
            }
        }
    }

    private void SpawnObject()
    {
        float randomY = Random.Range(spawnYRange.x, spawnYRange.y);
        Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);

        waves[waveNumber].SpawnedObjectCount++;

        NetworkObject obj = Instantiate(waves[waveNumber].prefab, spawnPos, Quaternion.identity);
        ServerManager.Spawn(obj); 
    }
}