using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public void GotoStart()
    {
        GameManager.Instance.ResetGame();
    }

    public void LoadNextScene()
    {
        GameManager.Instance.LoadNextScene();
    }

    public void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
    }

    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
    }

    public void HelpWindow()
    {
        GameManager.Instance.HelpWindow();
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }
}

