using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameManager : NetworkBehaviour
{
    [Header("Singleton")]
    public static GameManager Instance;

    [Header("Config")]
    private readonly SyncVar<float> syncWorldSpeed = new SyncVar<float>(
        new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers)
    );

    public float worldSpeed
    {
        get => syncWorldSpeed.Value;
        set
        {
            if (IsServerInitialized)
            {
                syncWorldSpeed.Value = value;
            }
            else
            {
                SetWorldSpeedServerRpc(value);
            }
        }
    }

    [SerializeField] private NetworkObject secretBoss;
    private bool bossSpawned = false;

    private readonly SyncVar<int> syncAlienCounter = new SyncVar<int>(
        new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers)
    );

    public int alienCounter
    {
        get => syncAlienCounter.Value;
        set
        {
            if (IsServerInitialized)
            {
                syncAlienCounter.Value = value;
            }
        }
    }

    [Header("Score")]
    private readonly SyncVar<int> syncScore = new SyncVar<int>(
        new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers)
    );

    public int Score
    {
        get => syncScore.Value;
        private set
        {
            if (IsServerInitialized)
            {
                syncScore.Value = value;
            }
        }
    }

    [Header("Score Values")]
    [SerializeField] private int alienKillScore;
    [SerializeField] private int obstacleDestroyScore;
    [SerializeField] private int bossKillScore;
    [SerializeField] private int enemyKillScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        syncWorldSpeed.OnChange += OnWorldSpeedChanged;
        syncScore.OnChange += OnScoreChanged; 
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        syncWorldSpeed.OnChange -= OnWorldSpeedChanged;
        syncScore.OnChange -= OnScoreChanged;
    }

    private void FixedUpdate()
    {
        if (!IsServerInitialized) return;

        if (alienCounter >= 10 && !bossSpawned) // Only for Demo
        {
            SpawnSecretBoss();
        }
    }

    private void SpawnSecretBoss()
    {
        if (secretBoss == null) return;


        Vector3 spawnPosition = new Vector3(15f, 0f, 0f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, -90f);

        NetworkObject boss = Instantiate(secretBoss, spawnPosition, spawnRotation);
        ServerManager.Spawn(boss);

        bossSpawned = true;
    }

    [Server]
    public void AddAlienKillScore()
    {
        Score += alienKillScore;
    }

    [Server]
    public void AddObstacleScore()
    {
        Score += obstacleDestroyScore;
    }

    [Server]
    public void AddBossKillScore()
    {
        Score += bossKillScore;
    }

    [Server]
    public void AddEnemyKillScore()
    {
        Score += enemyKillScore;
    }

    [Server]
    public void AddCustomScore(int points)
    {
        Score += points;
    }

    private void OnScoreChanged(int oldValue, int newValue, bool asServer)
    {

        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateScore(newValue);
        }
    }

    private void OnWorldSpeedChanged(float oldValue, float newValue, bool asServer)
    {
    // Debug.Log($"World Speed changed from {oldValue} to {newValue}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWorldSpeedServerRpc(float newSpeed)
    {
        syncWorldSpeed.Value = newSpeed;
    }
}