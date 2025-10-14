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
        isInConfig = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (configMenuUI != null) configMenuUI.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        isPaused = true;
        isInConfig = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Options()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (configMenuUI != null) configMenuUI.SetActive(true);

        isInConfig = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitOptions()
    {
        if (configMenuUI != null) configMenuUI.SetActive(false);
        isInConfig = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
