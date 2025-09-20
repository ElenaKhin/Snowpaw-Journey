using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;   // drag in Inspector, or auto-find

    private void Awake()
    {
        if (!audioManager) audioManager = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        // resume bgm
        if (audioManager) audioManager.ResumeMusic();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        // pause bgm
        if (audioManager) audioManager.PauseMusic();
    }

    public void LoadMenu()
    {
        // make sure time runs in the next scene
        Time.timeScale = 1f;

        // optional: resume/stop music depending on your menu design
        if (audioManager) audioManager.ResumeMusic();

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }

    // Optional: hook this to your UI Button OnClick to play a click sound
    public void PlayClickSFX()
    {
        if (audioManager) audioManager.PlayClick();
    }
}
