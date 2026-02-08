using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnNewGameButton()
    {
        SceneManager.LoadScene("Level 1"); 
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("MainMenu");// Back to the root
    }

    public void OnQuitButton()
    {
        Application.Quit();
        Debug.Log("Bis später");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}