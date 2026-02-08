using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameOverDelayed(float delay)
    {
        StartCoroutine(LoadGameOverCoroutine(delay));
    }

    private IEnumerator LoadGameOverCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnitySceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
    }
}