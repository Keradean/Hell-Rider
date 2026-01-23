using FishNet.Managing.Server;
using UnityEngine;
using FishNet.Object;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (!IsServerInitialized) return;

        // Nach der animation Despawnen
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(DespawnObject), animLength);
    }

    private void DespawnObject()
    {
        if (IsSpawned)
        {
            ServerManager.Despawn(gameObject);
        }
    }
}
