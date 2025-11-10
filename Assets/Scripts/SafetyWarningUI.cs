using System.Collections;
using UnityEngine;
using UnityEngine.UI;
// Se usi TextMeshPro, decommenta la riga qui sotto e i riferimenti nel codice.
// using TMPro;

public class SafetyWarningUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Il Panel UI del warning (GameObject figlio del tuo Canvas). Se vuoto, usa questo GameObject.")]
    [SerializeField] private GameObject warningPanel;

    [Tooltip("Componente Text standard (opzionale). Se usi TMP, lascia vuoto questo e usa tmpText.")]
    [SerializeField] private Text uiText;

    // Se usi TMP, decommenta queste due righe:
    // [Tooltip("Componente TMP_Text (opzionale). Se usi TMP, assegna questo e lascia uiText vuoto.")]
    // [SerializeField] private TMP_Text tmpText;

    [Header("Messaggio")]
    [Tooltip("Testo di default mostrato nel warning se non ne viene passato uno custom.")]
    [SerializeField] private string defaultText = "Fai attenzione!";

    [Header("Animazione (senza CanvasGroup)")]
    [Tooltip("Tempo in secondi durante il quale il pannello resta visibile.")]
    [SerializeField] private float visibleTime = 1.2f;

    [Tooltip("Durata dell'effetto 'pop' in apertura (secondi).")]
    [SerializeField] private float popInTime = 0.15f;

    [Tooltip("Scala iniziale del pannello per l'effetto pop (1 = dimensione reale).")]
    [SerializeField] private float popStartScale = 0.6f;

    [Tooltip("Durata dell'effetto 'pop out' finale (0 = scomparsa istantanea).")]
    [SerializeField] private float popOutTime = 0.1f;

    [Header("Antiflood")]
    [Tooltip("Tempo minimo (secondi) tra due avvisi consecutivi.")]
    [SerializeField] private float cooldown = 1.0f;

    private float _lastShowTime = -999f;
    private Coroutine _showRoutine;
    private Vector3 _originalScale;

    private void Awake()
    {
        // Se non impostato, usa il GameObject su cui è attaccato questo script
        if (warningPanel == null)
            warningPanel = gameObject;

        // Assicurati che all'avvio sia nascosto
        warningPanel.SetActive(false);

        // Memorizza la scala originale
        var rt = warningPanel.transform as RectTransform;
        _originalScale = rt != null ? rt.localScale : Vector3.one;

        // Consistenza: resetta la scala
        if (rt != null) rt.localScale = _originalScale;
    }

    /// <summary>
    /// Mostra il warning con animazione. Se customText è non nullo/vuoto, sostituisce il testo di default.
    /// </summary>
    public void ShowWarning(string customText = null)
    {
        if (Time.unscaledTime < _lastShowTime + cooldown)
            return;

        _lastShowTime = Time.unscaledTime;

        // Imposta il testo (se presente)
        if (!string.IsNullOrEmpty(customText))
            SetMessage(customText);
        else
            SetMessage(defaultText);

        // Ferma una eventuale animazione precedente
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        _showRoutine = StartCoroutine(ShowRoutine());
    }

    /// <summary>
    /// Imposta il messaggio visualizzato (supporta Text standard o TMP se attivo).
    /// </summary>
    public void SetMessage(string message)
    {
        if (uiText != null)
            uiText.text = message;

        // Se usi TMP, decommenta:
        // if (tmpText != null)
        //     tmpText.text = message;
    }

    /// <summary>
    /// Nasconde immediatamente il pannello (senza animazioni).
    /// </summary>
    public void HideImmediate()
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        if (warningPanel != null)
        {
            var rt = warningPanel.transform as RectTransform;
            if (rt != null) rt.localScale = _originalScale;
            warningPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Consente di aggiornare dinamicamente parametri principali a runtime (opzionale).
    /// </summary>
    public void Configure(float newVisibleTime, float newCooldown, float newPopInTime = -1f, float newPopOutTime = -1f)
    {
        visibleTime = Mathf.Max(0f, newVisibleTime);
        cooldown = Mathf.Max(0f, newCooldown);
        if (newPopInTime >= 0f) popInTime = newPopInTime;
        if (newPopOutTime >= 0f) popOutTime = newPopOutTime;
    }

    private IEnumerator ShowRoutine()
    {
        if (warningPanel == null)
            yield break;

        // Attiva il pannello e avvia pop-in
        warningPanel.SetActive(true);

        RectTransform rt = warningPanel.transform as RectTransform;
        if (rt == null)
        {
            // Se non è un RectTransform, gestisci solo visibilità temporizzata
            yield return new WaitForSecondsRealtime(visibleTime);
            warningPanel.SetActive(false);
            yield break;
        }

        // POP-IN
        Vector3 finalScale = _originalScale;
        Vector3 startScale = finalScale * Mathf.Max(0.01f, popStartScale);
        float t = 0f;
        float durationIn = Mathf.Max(0.0001f, popInTime);
        rt.localScale = startScale;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / durationIn;
            rt.localScale = Vector3.Lerp(startScale, finalScale, t);
            yield return null;
        }
        rt.localScale = finalScale;

        // TEMPO DI VISIBILITÀ
        if (visibleTime > 0f)
            yield return new WaitForSecondsRealtime(visibleTime);

        // POP-OUT (opzionale)
        if (popOutTime > 0f)
        {
            t = 0f;
            Vector3 start = rt.localScale;
            Vector3 end = Vector3.zero;
            float durationOut = popOutTime;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / durationOut;
                rt.localScale = Vector3.Lerp(start, end, t);
                yield return null;
            }
        }

        // Disattiva e ripristina scala
        warningPanel.SetActive(false);
        rt.localScale = _originalScale;

        _showRoutine = null;
    }
}