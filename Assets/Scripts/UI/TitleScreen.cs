using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public GameObject creditsScreen;
    public GameObject mainScreen;
    public void StartGame()
    {
        // TODO: Load the Starting scene, i don't know how this will be done
        SceneManager.LoadScene("1_WarRoom");
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void MoveToCredits()
    {
        creditsScreen.SetActive(true);
        mainScreen.SetActive(false);
    }

    public void Back()
    {
        creditsScreen.SetActive(false);
        mainScreen.SetActive(true);
    }
}
