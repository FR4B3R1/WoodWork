using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverTrigger : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Minigiochi")]
    [SerializeField] private CameraSwitcher minigame1Controller;
    [SerializeField] private CameraSwitcherSecondMinigame minigame2Controller;

    [Header("UI Avviso")]
    [SerializeField] private GameObject warningPanel; // Pannello animato in scena
    [SerializeField] private float warningDuration = 3f; // Durata in secondi

    [Header("UI Vittoria")]
    [SerializeField] private GameObject victoryPanel; // Pannello "Hai vinto!"
    [SerializeField] private float victoryDuration = 4f; // Durata in secondi

    private bool playerInside = false;

    void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed += ctx => Interagisci();
    }

    void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed -= ctx => Interagisci();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Il giocatore è entrato nel collider. Premi Interact.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            NascondiWarning();
        }
    }

    void Interagisci()
    {
        if (!playerInside) return;

        if (MinigamesCompletati())
        {
            AttivaGameOver();
        }
        else
        {
            MostraWarning();
            Debug.Log("Non puoi uscire: completa entrambi i minigiochi.");
        }
    }

    bool MinigamesCompletati()
    {
        return minigame1Controller != null && minigame1Controller.Minigame1Played
            && minigame2Controller != null && minigame2Controller.Minigame2Played;
    }

    void AttivaGameOver()
    {
        Debug.Log("Game Over attivato!");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            StartCoroutine(ChiudiDopoDelay(victoryDuration));
        }
        else
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void MostraWarning()
    {
        if (warningPanel == null) return;

        warningPanel.SetActive(true);
        StopAllCoroutines(); // Evita conflitti se chiamato più volte
        StartCoroutine(HideWarningAfterDelay(warningDuration));
    }

    void NascondiWarning()
    {
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    System.Collections.IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NascondiWarning();
    }

    System.Collections.IEnumerator ChiudiDopoDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
