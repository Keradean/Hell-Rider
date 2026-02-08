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
    public AudioSource hitObst;
    public AudioSource Shoot;
    public AudioSource EnemyDeath2;
    public AudioSource Burn;
    public AudioSource BossHit;
    public AudioSource bossCharge;


    // Ensures that there is only one instance of AudioManager 
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
    public void PlayTunedSound(AudioSource sound)
    {
        sound.pitch = Random.Range(0.8f, 1.2f);
        sound.Stop();
        sound.Play(); 

    }
}
