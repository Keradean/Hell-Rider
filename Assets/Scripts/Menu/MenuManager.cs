using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnNewGameButton()
    {
        SceneManager.LoadScene(2); // erstellt die Szene mit dem Index 2 bis Lobby fertig gestellt worden ist 
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene(0);// Back to the root
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