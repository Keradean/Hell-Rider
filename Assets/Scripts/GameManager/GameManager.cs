using UnityEngine;
using FishNet.Object;

public class GameManager : NetworkBehaviour
{
    [Header("Singleton")]
    public static GameManager Instance;

    [Header("Config")]
    public float worldSpeed; 

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
}