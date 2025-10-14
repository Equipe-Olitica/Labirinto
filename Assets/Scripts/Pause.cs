using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject configMenuUI;
    public static bool isPaused = false;
    public static bool isInConfig = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (isInConfig)
                    ExitOptions();
                else
                     Resume();
            }
            else
                Pause();
        }
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Options()
    {
        pauseMenuUI.SetActive(false);
        isInConfig = true;
    }

    public void ExitOptions()
    {
        configMenuUI.SetActive(false);
        isInConfig = false;
        Resume();
    }
}
