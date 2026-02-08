using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;

public class ObjectSpawner : NetworkBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnYRange = new Vector2(-5f, 4f);

    [Header("Wave Spawning")]
    [SerializeField] private readonly SyncVar<int> waveNumber = new SyncVar<int>(0);
    [SerializeField] private List<Wave> waves;

    // Liste aller aktiven Gegner dieser Wave
    private readonly SyncList<NetworkObject> activeEnemies = new SyncList<NetworkObject>();

    [System.Serializable]
    public class Wave
    {
        public NetworkObject prefab;
        public float spawnTimer;
        public float spawnInterval;
        public int objectsPerWave;
        [HideInInspector] public int SpawnedObjectCount;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (waves != null && waves.Count > 0 && waveNumber.Value < waves.Count)
        {
            waves[waveNumber.Value].spawnTimer = waves[waveNumber.Value].spawnInterval;
        }

        waveNumber.OnChange += OnWaveNumberChanged;
        UpdateWaveUI();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        waveNumber.OnChange += OnWaveNumberChanged;
        UpdateWaveUI();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        waveNumber.OnChange -= OnWaveNumberChanged;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        waveNumber.OnChange -= OnWaveNumberChanged;
    }

    void Update()
    {
        if (!IsServerInitialized) return;
        if (waves == null || waves.Count == 0) return;
        if (waveNumber.Value >= waves.Count) return;

        Wave currentWave = waves[waveNumber.Value];

        // Entferne null-Referenzen (zerstörte Gegner)
        CleanupDestroyedEnemies();

        // Spawne nur, wenn noch nicht alle Objekte gespawnt wurden
        if (currentWave.SpawnedObjectCount < currentWave.objectsPerWave)
        {
            float speedMultiplier = 1f;
            if (GameManager.Instance != null)
            {
                float currentWorldSpeed = GameManager.Instance.worldSpeed;
                speedMultiplier = currentWorldSpeed <= -10f ? 5f : 1f;
            }

            currentWave.spawnTimer += Time.deltaTime * speedMultiplier;

            if (currentWave.spawnTimer >= currentWave.spawnInterval)
            {
                SpawnObject();
                currentWave.spawnTimer = 0;
            }
        }
        // Prüfe, ob alle Objekte gespawnt UND zerstört wurden
        else if (currentWave.SpawnedObjectCount >= currentWave.objectsPerWave && activeEnemies.Count == 0)
        {
            // Nächste Wave starten
            currentWave.SpawnedObjectCount = 0;
            waveNumber.Value++;

            if (waveNumber.Value >= waves.Count)
            {
                waveNumber.Value = 0;
            }
        }
    }

    private void SpawnObject()
    {
        if (waveNumber.Value >= waves.Count) return;

        Wave currentWave = waves[waveNumber.Value];
        if (currentWave.prefab == null) return;

        float randomY = Random.Range(spawnYRange.x, spawnYRange.y);
        Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);

        currentWave.SpawnedObjectCount++;

        NetworkObject obj = Instantiate(currentWave.prefab, spawnPos, Quaternion.identity);
        ServerManager.Spawn(obj);

        // Füge zur Liste der aktiven Gegner hinzu
        activeEnemies.Add(obj);
    }

    // Entfernt zerstörte/null Gegner aus der Liste
    private void CleanupDestroyedEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    private void OnWaveNumberChanged(int oldValue, int newValue, bool asServer)
    {
        UpdateWaveUI();
    }

    private void UpdateWaveUI()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateWaveText(waveNumber.Value);
        }
    }
}