using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels (will auto-find)")]
    private GameObject pauseMenuUI;
    private GameObject gameOverUI;
    private GameObject winMenuUI;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [HideInInspector] public bool GameIsPaused = false;
    [HideInInspector] public bool GameIsOver = false;
    [HideInInspector] public bool GameIsWon = false;

    private void Awake()
    {
        if (!audioManager)
            audioManager = FindObjectOfType<AudioManager>();
    }

    private void Start()
    {
        // Reset states
        GameIsOver = false;
        GameIsPaused = false;
        GameIsWon = false;
        Time.timeScale = 1f;

        // Auto-find panels in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            pauseMenuUI = canvas.transform.Find("PauseMenu")?.gameObject;
            gameOverUI = canvas.transform.Find("GameOverMenu")?.gameObject;
            winMenuUI = canvas.transform.Find("WinMenu")?.gameObject;

            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            else Debug.LogWarning("PauseMenu not found under Canvas!");

            if (gameOverUI != null) gameOverUI.SetActive(false);
            else Debug.LogWarning("GameOverMenu not found under Canvas!");

            if (winMenuUI != null) winMenuUI.SetActive(false);
            else Debug.LogWarning("WinMenu not found under Canvas!");
        }
        else
        {
            Debug.LogWarning("No Canvas found in scene!");
        }
    }

    private void Update()
    {
        if (GameIsOver || GameIsWon) return;

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) ResumeGame();
            else PauseGame();
        }
    }

    #region Pause
    public void PauseGame()
    {
        if (pauseMenuUI) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (audioManager) audioManager.PauseMusic();
    }

    public void ResumeGame()
    {
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (audioManager) audioManager.ResumeMusic();
    }
    #endregion

    #region GameOver
    public void GameOver()
    {
        GameIsOver = true;
        if (gameOverUI) gameOverUI.SetActive(true);
        Time.timeScale = 0f;

        if (audioManager) audioManager.PauseMusic();
    }
    #endregion

    #region Win
    public void WinGame()
    {
        GameIsWon = true;
        if (winMenuUI) winMenuUI.SetActive(true);
        Time.timeScale = 0f;

        if (audioManager) audioManager.PauseMusic();
    }
    #endregion

    #region Scene Control
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        GameIsOver = false;
        GameIsWon = false;
        SceneManager.LoadScene("Level1");
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        GameIsOver = false;
        GameIsWon = false;
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }

    public void PlayClickSFX()
    {
        if (audioManager) audioManager.PlayClick();
    }
}
