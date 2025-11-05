using UnityEngine;

public class Quitgame : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();

        Debug.LogWarning("Game is exiting...");
    }
}
