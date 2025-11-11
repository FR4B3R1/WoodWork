using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class MinigameUIController : MonoBehaviour
{
    [Header("Panel UI del minigioco")]
    [SerializeField] private GameObject panel; // Il root del pannello (il GameObject del Panel)

    [Header("Warning UI")]
    [SerializeField] private GameObject warningPanel; // Pannello per il messaggio di avviso
    [SerializeField] private Text warningText;        // Testo del messaggio

    [Header("Eventi")]
    public UnityEvent OnStartPressed; // da collegare a CameraSwitcher.StartMinigame

    private void Awake()
    {
        if (panel == null) panel = gameObject;
        panel.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);
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

    // ✅ Metodo per mostrare il messaggio di equipaggiamento mancante
    public void ShowEquipWarning(string message)
    {
        if (warningPanel == null || warningText == null) return;

        warningText.text = message;
        warningPanel.SetActive(true);

        // Nascondi dopo 2 secondi
        StartCoroutine(HideWarningAfterDelay(2f));
    }

    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningPanel.SetActive(false);
    }
}