using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Singleton")]
    public static AudioManager Instance { get; private set; }
    [Header("Effects")]
    public AudioSource Death;
    public AudioSource EnemyDeath;
    public AudioSource Fire;
    public AudioSource Hit;
    public AudioSource Boost;
    public AudioSource Pause;
    public AudioSource Unpause;


    // Ensures that there is only one instance of AudioManager (Singleton Pattern)
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void PlaySound(AudioSource sound)
    {
        sound.Stop();
        sound.Play(); 

    }
}
