using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        // TODO: Load the Starting scene, i don't know how this will be done
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
