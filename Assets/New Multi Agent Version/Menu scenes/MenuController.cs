using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class MenuController : MonoBehaviour
{
    // Loads the gameplay scene
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    // Reloads the gameplay scene to try again
    public void RetryGame()
    {
        SceneManager.LoadScene("Game");
    }

    // Returns the player back to the main menu screen
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Exits the built application
    public void QuitGame()
    {
        Debug.Log("Quit Button Pressed!"); // Confirms execution in editor
        Application.Quit(); // Closes the running build
    }
}
