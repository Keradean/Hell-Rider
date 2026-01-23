using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Singleton")]
    public static UIController Instance;

    [Header("UI Elements")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text scoreText;  // ✅ NEU
    public GameObject pausePanel;

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

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        UpdateScore(0);
    }

    public void UpdateEnergyBar(float current, float max)
    {
        if (energyBar == null || energyText == null) return;

        energyBar.maxValue = max;
        energyBar.value = current;
        energyText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }

    public void UpdateHealthBar(float current, float max)
    {
        if (healthBar == null || healthText == null) return;

        healthBar.maxValue = max;
        healthBar.value = current;
        healthText.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }

    public void OnResumeButton()
    {
        if (PlayerController.localInstance != null)
        {
            PlayerController.localInstance.Pause();
        }
    }

    public void UpdateWaveText(int waveNumber)
    {
        if (waveText == null) return;
        waveText.text = $"Wave {waveNumber + 1}";
    }
    public void UpdateScore(int score)
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {score:N0}";
    }
}