using UnityEngine;
using UnityEngine.Events;

public class MinigameUIController : MonoBehaviour
{
    [Header("Panel UI del minigioco")]
    [SerializeField] private GameObject panel; // Il root del pannello (il GameObject del Panel)

    [Header("Eventi")]
    public UnityEvent OnStartPressed; // da collegare a CameraSwitcher.StartMinigame

    private void Awake()
    {
        if (panel == null) panel = gameObject;
        // Assicurati che all'avvio sia spento
        panel.SetActive(false);
    }

    public void ShowPanel()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    // Chiamato dal bottone "Start"
    public void OnPressStart()
    {
        HidePanel();
        OnStartPressed?.Invoke();
    }
}