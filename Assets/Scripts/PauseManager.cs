using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Pannello Pausa")]
    [SerializeField] private GameObject pausePanel;

    [Header("Player Controller")]
    [SerializeField] private MonoBehaviour playerController; // Assegna il tuo FirstPersonController
    [SerializeField] private MonoBehaviour cameraLookController; // Se hai uno script separato per la visuale

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Ferma il tempo di gioco
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Disattiva movimento e visuale
        if (playerController != null) playerController.enabled = false;
        if (cameraLookController != null) cameraLookController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Riprende il tempo di gioco
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Riattiva movimento e visuale
        if (playerController != null) playerController.enabled = true;
        if (cameraLookController != null) cameraLookController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("menu");
    }
}