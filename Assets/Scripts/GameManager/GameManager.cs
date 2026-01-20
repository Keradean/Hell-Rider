using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameManager : NetworkBehaviour
{
    [Header("Singleton")]
    public static GameManager Instance;

    [Header("Config")]
    private readonly SyncVar<float> syncWorldSpeed = new SyncVar<float>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

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
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        syncWorldSpeed.OnChange -= OnWorldSpeedChanged;
    }

    private void OnWorldSpeedChanged(float oldValue, float newValue, bool asServer)
    {
        Debug.Log($"World Speed changed from {oldValue} to {newValue}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWorldSpeedServerRpc(float newSpeed)
    {
        syncWorldSpeed.Value = newSpeed;
    }

}