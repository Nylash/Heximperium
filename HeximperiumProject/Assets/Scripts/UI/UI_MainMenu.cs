using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public void LaunchGame()
    {
        SceneManager.LoadScene("Test");
    }

    public void QuitGame()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfav2yqM8XQFg-BkDHh5HvbugKSOXGCSP6hiaSW58-OyttKgQ/viewform?usp=sharing&ouid=102342740940582761191");
        Application.Quit();
    }
}
