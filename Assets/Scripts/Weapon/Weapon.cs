using UnityEngine;

public class Weapon : MonoBehaviour
{
    public static Weapon Instance;

    [Header("Weapon")]
    [SerializeField] private GameObject prefab; 

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

}
