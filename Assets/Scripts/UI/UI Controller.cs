using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Singleton")]
    public static UIController Instance;

    [Header("UI Elements")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    //[SerializeField] private TMP_Text scoreText;

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

    public void UpdateEnegeryBar(float current, float max)
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
}